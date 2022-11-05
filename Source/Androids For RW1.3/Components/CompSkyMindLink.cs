using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Verse.AI.Group;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using Verse.Sound;
using RimWorld.Planet;
using System.Text.RegularExpressions;
using RimWorld.BaseGen;

namespace ATReforged
{
    public class CompSkyMindLink : ThingComp
    {
        private Pawn ThisPawn => (Pawn)parent;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref networkOperationInProgress, "ATR_networkOperationInProgress", -1);
            Scribe_Values.Look(ref controlMode, "ATR_controlMode", false);
            Scribe_Values.Look(ref isForeign, "ATR_isForeign", false);

            // Only save and load recipient data if this comp is registered as being in a mind operation.
            if (networkOperationInProgress > -1)
            {
                Scribe_References.Look(ref recipientPawn, "ATR_recipientPawn");
            }
            // Only save and load surrogate data if this comp is attached to a player pawn and has or is a surrogate.
            if (!isForeign && Linked == -2)
            {
                Scribe_Collections.Look(ref surrogatePawns, "ATR_surrogatePawns", lookMode: LookMode.Reference);
            }
        }

        public override void PostDraw()
        {
            Material avatar = null;
            Vector3 vector;

            if (Linked > -1)
                avatar = Tex.MindOperation;
            else if (HasSurrogate() || isForeign)
                avatar = Tex.RemotelyControlledNode;

            if (avatar != null)
            {
                vector = parent.TrueCenter();
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28125f;
                vector.z += 1.4f;
                vector.x += parent.def.size.x / 2;

                Graphics.DrawMesh(MeshPool.plane08, vector, Quaternion.identity, avatar, 0);
            }
        }


        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (controlMode)
            {
                ToggleControlMode();
            }
            if (Linked > -1)
            {
                Log.Warning("[ATR] Destroyed a pawn mid-mind operation. This may not be a clean interrupt, issues may arise.");
                HandleInterrupt();
            }
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            if (HasSurrogate() && !isForeign)
            {
                foreach (Pawn surrogate in surrogatePawns)
                {
                    if (ThisPawn.Spawned && surrogate.Map == parent.Map)
                    {
                        GenDraw.DrawLineBetween(parent.TrueCenter(), surrogate.TrueCenter(), SimpleColor.Blue);
                    }
                }
            }
        }

        // Toggle whether this pawn is designated as a surrogate controller or not. Surrogate controllers are downed.
        public void ToggleControlMode()
        {
            controlMode = !controlMode;
            if (!controlMode && HasSurrogate())
            {
                DisconnectSurrogates();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // No point in showing SkyMind operations on a pawn that isn't connected to it or is not a valid target for mind operations.
            if (!Utils.IsValidMindTransferTarget(ThisPawn))
                yield break;

            // Prisoners may have their mind absorbed for server points. No other operations are legal on them.
            if (ThisPawn.IsPrisonerOfColony)
            {
                if (Linked == -1 && (Utils.gameComp.GetSkillPointCapacity() > 0 || Utils.gameComp.GetHackingPointCapacity() > 0))
                {
                    Texture2D tex = Tex.MindAbsorption;
                    yield return new Command_Action
                    {
                        icon = tex,
                        defaultLabel = "ATR_AbsorbExperience".Translate(),
                        defaultDesc = "ATR_AbsorbExperienceDesc".Translate(),
                        action = delegate ()
                        {
                            Find.WindowStack.Add(new Dialog_Msg("ATR_AbsorbExperience".Translate(), "ATR_AbsorbExperienceConfirm".Translate(parent.LabelShortCap) + "\n" + ("ATR_SkyMindDisconnectionRisk").Translate(), delegate
                            {
                                Linked = 3;
                            }, false));
                        }
                    };
                }

                // Skip all other operations. They are illegal on prisoners.
                yield break;
            }

            // Surrogate only operations.
            if (Utils.IsSurrogate(ThisPawn))
            {
                //Organic surrogates may receive downloads from the SkyMind network.
                if (!Utils.IsConsideredMechanical(ThisPawn) && Utils.gameComp.GetCloudPawns().Count > 0)
                yield return new Command_Action
                {
                    icon = Tex.DownloadFromSkyCloud,
                    defaultLabel = "ATR_DownloadCloudPawn".Translate(),
                    defaultDesc = "ATR_DownloadCloudPawnDesc".Translate(),
                    action = delegate ()
                    {
                        List<FloatMenuOption> opts = new List<FloatMenuOption>();

                        foreach (Pawn pawn in Utils.gameComp.GetCloudPawns())
                        {
                            // Skip intelligences that are busy with other mind operations or that are busy controlling surrogates.
                            if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation) != null || pawn.TryGetComp<CompSkyMindLink>().HasSurrogate())
                                continue;

                            opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate ()
                            {
                                Find.WindowStack.Add(new Dialog_Msg("ATR_DownloadCloudPawn".Translate(), "ATR_DownloadCloudPawnConfirm".Translate() + "\n" + ("ATR_SkyMindDisconnectionRisk").Translate(), delegate
                                {
                                    recipientPawn = pawn;
                                    Linked = 4;
                                }));
                            }));
                        }
                        opts.SortBy((x) => x.Label);

                        if (opts.Count == 0)
                            opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                        Find.WindowStack.Add(new FloatMenu(opts, ""));
                    }
                };

                // Surrogates may disconnect freely from their host.
                yield return new Command_Action
                {
                    icon = Tex.DisconnectIcon,
                    defaultLabel = "ATR_DisconnectSurrogate".Translate(),
                    defaultDesc = "ATR_DisconnectSurrogateDesc".Translate(),
                    action = delegate ()
                    {
                        Utils.gameComp.DisconnectFromSkyMind(ThisPawn);
                    }
                };

                // Skip all other operations. They are illegal on surrogates.
                yield break;
            }

            // Always show surrogate control mode, as any non-surrogate SkyMind connected pawn may use them.
            yield return new Command_Toggle
            {
                icon = Tex.ControlModeIcon,
                defaultLabel = "ATR_ToggleControlMode".Translate(),
                defaultDesc = "ATR_ToggleControlModeDesc".Translate(),
                isActive = () => controlMode,
                toggleAction = delegate ()
                {
                    ToggleControlMode();
                }
            };

            // Always show Skill Up menu option, as any non-surrogate SkyMind connected pawn may use them.
            yield return new Command_Action
            {
                icon = Tex.SkillWorkshopIcon,
                defaultLabel = "ATR_Skills".Translate(),
                defaultDesc = "ATR_SkillsDesc".Translate(),
                action = delegate ()
                {
                    Find.WindowStack.Add(new Dialog_SkillUp((Pawn)parent));
                }
            };

            // If in surrogate control mode, allow selecting a surrogate to control and allow updating surrogate subconsciousness. No other operations are legal while in control mode.
            if (controlMode)
            {
                // Allow connecting to new surrogates.
                yield return new Command_Action
                {
                    icon = Tex.ConnectIcon,
                    defaultLabel = "ATR_ControlSurrogate".Translate(),
                    defaultDesc = "ATR_ControlSurrogateDesc".Translate(),
                    action = delegate ()
                    {
                        List<FloatMenuOption> opts = new List<FloatMenuOption>();
                        foreach (Map map in Find.Maps)
                        {
                            opts.Add(new FloatMenuOption(map.Parent.Label, delegate
                            {
                                Current.Game.CurrentMap = map;
                                Designator_AndroidToControl x = new Designator_AndroidToControl((Pawn)parent);
                                Find.DesignatorManager.Select(x);

                            }));
                        }
                        if (opts.Count == 1)
                        {
                            Designator_AndroidToControl x = new Designator_AndroidToControl((Pawn)parent);
                            Find.DesignatorManager.Select(x);
                        }
                        else if (opts.Count > 1)
                        {
                            FloatMenu floatMenuMap = new FloatMenu(opts);
                            Find.WindowStack.Add(floatMenuMap);
                        }
                    }
                };

                // Allow connecting a host to hostless surrogates in caravans.
                IEnumerable<Pawn> hostlessSurrogatesInCaravans = Utils.GetHostlessCaravanSurrogates();

                if (hostlessSurrogatesInCaravans != null)
                {
                    yield return new Command_Action
                    {
                        icon = Tex.RecoveryIcon,
                        defaultLabel = "ATR_ControlCaravanSurrogate".Translate(),
                        defaultDesc = "ATR_ControlCaravanSurrogateDesc".Translate(),
                        action = delegate ()
                        {
                            List<FloatMenuOption> opts = new List<FloatMenuOption>();
                            foreach (Pawn surrogate in hostlessSurrogatesInCaravans)
                            {
                                opts.Add(new FloatMenuOption(surrogate.LabelShortCap, delegate ()
                                {
                                    if (!Utils.gameComp.AttemptSkyMindConnection(surrogate))
                                        return;
                                    ConnectSurrogate(surrogate);
                                }));
                            }
                            opts.SortBy((x) => x.Label);
                            Find.WindowStack.Add(new FloatMenu(opts, ""));
                        }
                    };
                }

                // A surrogate is connected. Allow disconnection from it.
                if (HasSurrogate())
                {
                    yield return new Command_Action
                    {
                        icon = Tex.DisconnectIcon,
                        defaultLabel = "ATR_DisconnectSurrogate".Translate(),
                        defaultDesc = "ATR_DisconnectSurrogateDesc".Translate(),
                        action = delegate ()
                        {
                            DisconnectSurrogates();
                        }
                    };
                }

                // Skip all other operations.
                yield break;
            }

            // Allow this pawn to do permutations.
            yield return new Command_Action
            {
                icon = Tex.Permute,
                defaultLabel = "ATR_Permute".Translate(),
                defaultDesc = "ATR_PermuteDesc".Translate(),
                action = delegate ()
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                    foreach (Pawn colonist in ThisPawn.Map.mapPawns.FreeColonists.Where(colonist => Utils.IsValidMindTransferTarget(colonist) && colonist != ThisPawn && !Utils.IsSurrogate(colonist)))
                    {
                        opts.Add(new FloatMenuOption(colonist.LabelShortCap, delegate ()
                        {
                            Find.WindowStack.Add(new Dialog_Msg("ATR_Permute".Translate(), "ATR_PermuteConfirm".Translate(parent.LabelShortCap, colonist.LabelShortCap) + "\n" + ("ATR_SkyMindDisconnectionRisk").Translate(), delegate
                            {
                                recipientPawn = colonist;
                                Linked = 1;
                            }, false));
                        }));
                    }
                    opts.SortBy((x) => x.Label);

                    if (opts.Count == 0)
                        opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                    Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableTargetPawns".Translate()));
                }
            };

            // Allow this pawn to do duplications.
            yield return new Command_Action
            {
                icon = Tex.Duplicate,
                defaultLabel = "ATR_Duplicate".Translate(),
                defaultDesc = "ATR_DuplicateDesc".Translate(),
                action = delegate ()
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                    foreach (Pawn colonist in ThisPawn.Map.mapPawns.FreeColonists.Where(colonist => Utils.IsValidMindTransferTarget(colonist) && colonist != ThisPawn && !Utils.IsConsideredMechanical(colonist)))
                    {
                        opts.Add(new FloatMenuOption(colonist.LabelShortCap, delegate ()
                        {
                            Find.WindowStack.Add(new Dialog_Msg("ATR_Duplicate".Translate(), "ATR_DuplicateConfirm".Translate(parent.LabelShortCap, colonist.LabelShortCap) + "\n" + ("ATR_SkyMindDisconnectionRisk").Translate(), delegate
                            {
                                recipientPawn = colonist;
                                Linked = 2;
                            }, false));
                        }));
                    }
                    opts.SortBy((x) => x.Label);

                    if (opts.Count == 0)
                        opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                    Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableTargetPawns".Translate()));
                }
            };

            // Uploading requires space in the SkyMind network for the intelligence.
            if (Utils.gameComp.GetSkyMindCloudCapacity() > Utils.gameComp.GetCloudPawns().Count)
            {
                yield return new Command_Action
                {
                    icon = Tex.SkyMindUpload,
                    defaultLabel = "ATR_Upload".Translate(),
                    defaultDesc = "ATR_UploadDesc".Translate(),
                    action = delegate ()
                    {
                        Find.WindowStack.Add(new Dialog_Msg("ATR_Upload".Translate(), "ATR_UploadConfirm".Translate() + "\n" + ("ATR_SkyMindDisconnectionRisk").Translate(), delegate
                        {
                            Linked = 5;
                        }));
                    }
                };
            }

            yield break;
        }

        // Controller for the state of viruses in the parent. -1 = clean, 1 = sleeper, 2 = cryptolocker, 3 = breaker. Ticker handled by the GC to avoid calculating when clean.
        public int Linked
        {
            get
            {
                return networkOperationInProgress;
            }

            set
            {
                int status = networkOperationInProgress;
                networkOperationInProgress = value;
                if (networkOperationInProgress == -1 && status > -1)
                { // Pawn's operation has ended. Close out appropriate function based on the networkOperation that had been chosen (contained in status).
                    HandleSuccess(status);

                    // All pawns undergo a system reboot upon successful completion of an operation.
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.ATR_ShortReboot, ThisPawn, null);
                    hediff.Severity = 1f;
                    ThisPawn.health.AddHediff(hediff, null, null);

                    Utils.gameComp.PopNetworkLinkedPawn(ThisPawn);
                }
                else if (networkOperationInProgress > -1)
                { // Operation has begun. Stand by until completion or aborted.
                    HandleInitialization();
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            if (parent.Map == null)
                return base.CompInspectStringExtra();

            // A SkyMind operation is in progress. State how long players must wait before the operation will be complete.
            if (networkOperationInProgress > -1 && Utils.gameComp.GetAllLinkedPawns().ContainsKey(ThisPawn))
            {
                ret.AppendLine("ATR_SkyMindOperationInProgress".Translate((Utils.gameComp.GetLinkedPawn(ThisPawn) - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose()));
            }
            else if (networkOperationInProgress == -2)
            {
                ret.AppendLine("ATR_SurrogateConnected".Translate(string.Join(", ", surrogatePawns)));
            }
            return ret.Append(base.CompInspectStringExtra()).ToString();
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            switch (signal)
            {
                case "SkyMindNetworkUserDisconnected":
                    // Disconnect any linked pawns, depending on the control mode of this pawn.
                    if (controlMode)
                    {
                        ToggleControlMode();
                    }
                    else
                    {
                        DisconnectController();
                    }

                    // Check to see if any mind operations were interrupted by the disconnection.
                    CheckInterruptedUpload();
                    break;
            }
        }

        // Connect to the provided surrogate, with this pawn as the controller.
        public void ConnectSurrogate(Pawn surrogate, bool external = false)
        {
            // Copy this pawn into the surrogate. Player surrogates are tethered to the controller.
            Utils.Duplicate(ThisPawn, surrogate, false, !external);

            // Foreign controllers aren't saved, so only handle linking the surrogate and controller together if it's a player pawn.
            if (!external)
            {
                surrogatePawns.Add(surrogate);
                surrogate.TryGetComp<CompSkyMindLink>().surrogatePawns.Add(ThisPawn);
                FleckMaker.ThrowDustPuffThick(surrogate.Position.ToVector3Shifted(), surrogate.Map, 4.0f, Color.blue);
                Messages.Message("ATR_SurrogateConnected".Translate(ThisPawn.LabelShortCap, surrogate.LabelShortCap), ThisPawn, MessageTypeDefOf.PositiveEvent);
                Linked = -2;
                surrogate.TryGetComp<CompSkyMindLink>().Linked = -2;
            }
            else
            {
                // Foreign controllers aren't saved, and are only needed to initialize the surrogate. Foreign surrogates operate independently until downed or killed.
                isForeign = true;
                surrogate.TryGetComp<CompSkyMindLink>().isForeign = true;
            }

            // Remove the surrogate's NoHost hediff.
            Hediff target = surrogate.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_NoController);
            if (target != null)
                surrogate.health.RemoveHediff(target);
        }

        // Return whether this pawn controls surrogates.
        public bool HasSurrogate()
        {
            return surrogatePawns.Any();
        }

        public IEnumerable<Pawn> GetSurrogates()
        { 
            return surrogatePawns; 
        }

        // Called only on controlled surrogates, this will deactivate the surrogate and inform the controller it was disconnected. External controllers (other factions) are deleted.
        public void DisconnectController()
        {
            if (!HasSurrogate() && !isForeign)
            {
                return;
            }

            // Apply the blank template to self.
            Utils.Duplicate(Utils.GetBlank(), ThisPawn, false);

            // Foreign surrogates do not have links to their controllers.
            if (!isForeign)
            {
                // Disconnect the surrogate from its controller.
                surrogatePawns.First().TryGetComp<CompSkyMindLink>().surrogatePawns.Remove(ThisPawn);
                surrogatePawns.Clear();
            }

            ThisPawn.guest?.SetGuestStatus(Faction.OfPlayer);
            if (ThisPawn.playerSettings != null)
                ThisPawn.playerSettings.medCare = MedicalCareCategory.Best;

            // Apply NoHost hediff to player surrogates.
            if (!isForeign)
            {
                ThisPawn.health.AddHediff(HediffDefOf.ATR_NoController);
                Linked = -1;
            }
        }

        // Called on surrogate controllers, this will disconnect all connected surrogates. It will do nothing if there are none.
        public void DisconnectSurrogates()
        {
            if (!HasSurrogate())
            {
                return;
            }

            // Disconnect each surrogate from the SkyMind (and this pawn by extension).
            foreach (Pawn surrogate in surrogatePawns.ToList())
            {
                Utils.gameComp.DisconnectFromSkyMind(surrogate);
            }
            // Forget about all surrogates.
            surrogatePawns.Clear();
        }

        // Applies some form of corruption to the provided pawn. For organics, this is dementia. For mechanicals, this is a slowly fading memory corruption.
        public void ApplyCorruption(Pawn pawn)
        {
            if (pawn == null)
                return;

            Hediff corruption;
            if (Utils.IsConsideredMechanical(pawn))
            {
                corruption = HediffMaker.MakeHediff(HediffDefOf.ATR_MemoryCorruption, pawn, pawn.health.hediffSet.GetBrain());
                corruption.Severity = Rand.Range(0.15f, 0.95f);
                pawn.health.AddHediff(corruption, pawn.health.hediffSet.GetBrain(), null);
            }
            else
            {
                corruption = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Dementia, pawn, null);
                corruption.Severity = Rand.Range(0.15f, 0.5f);
                pawn.health.AddHediff(corruption, null, null);
            }

            // Pawn loses corruption severity as a percent of total xp in each skill.
            foreach (SkillRecord skillRecord in pawn.skills.skills)
            {
                skillRecord.Learn((float)(-skillRecord.XpTotalEarned * corruption.Severity), true);
            }
        }

        // An operation has been started. Handle the outcome based on which operation has been initialized.
        public void HandleInitialization()
        {
            ThisPawn.health.AddHediff(HediffDefOf.ATR_MindOperation);
            Utils.gameComp.PushNetworkLinkedPawn(ThisPawn, Find.TickManager.TicksGame + ATReforged_Settings.timeToCompleteSkyMindOperations * 2500);
            if (recipientPawn != null)
            {
                recipientPawn.health.AddHediff(HediffDefOf.ATR_MindOperation);
            }
            Messages.Message("ATR_OperationInitiated".Translate(ThisPawn.LabelShort), parent, MessageTypeDefOf.PositiveEvent);
        }

        // An operation was interrupted while in progress. Handle the outcome based on which operation was occurring.
        public void HandleInterrupt()
        {
            // Permutation failure corrupts both pawns. SkyMind intelligences are protected against such corruptions as they lack physical memory and have back-ups.
            if (Linked == 1)
            {
                ApplyCorruption(ThisPawn);
                ApplyCorruption(recipientPawn);

                recipientPawn.TryGetComp<CompSkyMindLink>().Linked = -1;
            }
            // Duplication or Upload failure corrupts the source pawn.
            else if (Linked == 2 || Linked == 5)
            {
                ApplyCorruption(ThisPawn);
                if (recipientPawn != null)
                    recipientPawn.TryGetComp<CompSkyMindLink>().Linked = -1;
            }
            // Absorption failure kills the source pawn - they were going to die on success any way.
            else if (Linked == 3)
            {
                Utils.gameComp.DisconnectFromSkyMind(ThisPawn);
                ThisPawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 99999f, 999f, -1f, null, ThisPawn.health.hediffSet.GetBrain()));
                // If they're somehow not dead from that, make them dead for real.
                if (!ThisPawn.Dead)
                {
                    ThisPawn.Kill(null);
                }
                return;
            }
            // Nothing happens on other failures (Download and Replication) as the current pawn has no functioning physical intelligence and clouds keep back ups.

            Utils.gameComp.PopNetworkLinkedPawn(ThisPawn);
            Linked = -1;
        }

        // Apply the correct effects upon successfully completing the SkyMind operation based on the status (parameter as Linked is changed to -1 just beforehand).
        public void HandleSuccess(int status)
        {
            Hediff target = ThisPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation);
            if (target != null)
                ThisPawn.health.RemoveHediff(target);

            // If there is a recipient pawn, it was a permutation, duplication, or download. Handle appropriately.
            if (recipientPawn != null)
            {
                target = recipientPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation);
                if (target != null)
                    recipientPawn.health.RemoveHediff(target);
                // Permutation, swap the pawn's minds.
                if (status == 1)
                {
                    Utils.PermutePawn(ThisPawn, recipientPawn);
                    recipientPawn.health.AddHediff(HediffDefOf.ATR_ShortReboot);
                }
                // Duplication, insert a copy of the current pawn into the recipient.
                else if (status == 2)
                {
                    Utils.Duplicate(ThisPawn, recipientPawn, false, false);
                    // Duplication may only occur via operation on organic surrogates. Remove the receiver (burns out), remove no host hediff.
                    target = recipientPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_SkyMindReceiver);
                    if (target != null)
                        recipientPawn.health.RemoveHediff(target);
                    target = recipientPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_NoController);
                    if (target != null)
                        recipientPawn.health.RemoveHediff(target);
                }
                // Download, insert of a copy of the recipient pawn into the current pawn. Then destroy the recipient (SkyMind intelligence).
                else if (status == 4)
                {
                    Utils.Duplicate(recipientPawn, ThisPawn, false, false);
                    Utils.gameComp.PopCloudPawn(recipientPawn);
                    recipientPawn.Destroy();
                    target = ThisPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_SkyMindReceiver);
                    if (target != null)
                        ThisPawn.health.RemoveHediff(target);
                    target = ThisPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_NoController);
                    if (target != null)
                        ThisPawn.health.RemoveHediff(target);
                }
            }

            // Absorption kills the source pawn and generates valuable hacking and skill points for the colony.
            if (status == 3)
            {
                int sum = 0;
                foreach (SkillRecord skillRecord in ThisPawn.skills.skills)
                {
                    // For each skill the pawn possesses, generate 10 * level ^ 1.5 points to be distributed between hacking and skill.
                    sum += (int)(Math.Pow(skillRecord.levelInt, 1.5) * 10);
                }

                Utils.gameComp.ChangeServerPoints(sum/2, ServerType.HackingServer);
                Utils.gameComp.ChangeServerPoints(sum/2, ServerType.SkillServer);
                ThisPawn.Kill(null);
            }

            // Upload moves the pawn to the SkyMind network (despawns and puts into storage).
            if (status == 5)
            {
                // Add the pawn to storage and suspend any tick-checks it performs.
                Utils.gameComp.PushCloudPawn(ThisPawn);
                Current.Game.tickManager.DeRegisterAllTickabilityFor(ThisPawn);

                // Remove all traits that pawns aren't allowed to have upon uploading.
                Utils.removeMindBlacklistedTrait(ThisPawn);

                // Upon completion, we need to spawn a copy of the pawn to take their physical place as the original pawn despawns "into" the SkyMind Core. 
                Pawn corpse = Utils.SpawnCopy(ThisPawn, ATReforged_Settings.uploadingToSkyMindKills);
                // If in the settings, uploading is set to Permakill, find the new pawn copy's brain and mercilessly destroy it so it can't be revived.
                if (ATReforged_Settings.uploadingToSkyMindPermaKills)
                {
                    corpse.TakeDamage(new DamageInfo(DamageDefOf.Burn, 99999f, 999f, -1f, null, corpse.health.hediffSet.GetBrain()));
                }

                // The pawn does not need to be connected to the SkyMind directly now, and should disappear.
                Utils.gameComp.DisconnectFromSkyMind(ThisPawn);
                ThisPawn.DeSpawn();
            }

            // Replication simply creates a new SkyMind intelligence duplicated from another.
            if (status == 6)
            {
                // Generate the clone. 
                PawnGenerationRequest request = new PawnGenerationRequest(ThisPawn.kindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, fixedBiologicalAge: ThisPawn.ageTracker.AgeBiologicalYearsFloat, fixedChronologicalAge: ThisPawn.ageTracker.AgeChronologicalYearsFloat, fixedGender: ThisPawn.gender, fixedMelanin: ThisPawn.story.melanin);
                Pawn clone = PawnGenerator.GeneratePawn(request);

                // Copy the name of this pawn into the clone.
                NameTriple newName = (NameTriple)ThisPawn.Name;
                clone.Name = new NameTriple(newName.First, newName.Nick + Rand.RangeInclusive(100, 999), newName.Last);

                // Remove any Hediffs the game may have applied when generating the clone - this is to avoid weird hediffs appearing that may cause unexpected behavior.
                clone.health.RemoveAllHediffs();

                // It should however have an Autonomous Core or Transceiver hediff, as this informs the game that it is SkyMind capable (which it definitely is).
                if (Utils.IsConsideredMechanical(ThisPawn))
                {
                    clone.health.AddHediff(HediffDefOf.ATR_AutonomousCore, clone.health.hediffSet.GetBrain());
                }
                else
                {
                    clone.health.AddHediff(HediffDefOf.ATR_SkyMindTransceiver, clone.health.hediffSet.GetBrain());
                }

                // Duplicate the intelligence of this pawn into the clone (not murder) and add them to the SkyMind network.
                Utils.Duplicate(ThisPawn, clone, false, false);
                Utils.gameComp.PushCloudPawn(clone);
            }

            Find.LetterStack.ReceiveLetter("ATR_OperationCompleted".Translate(), "ATR_OperationCompletedDesc".Translate(ThisPawn.LabelShortCap), LetterDefOf.PositiveEvent, ThisPawn);
        }
        

        // Check if there is an operation in progress. If there is (Linked != -1) and it is the operation source (LinkedPawn != -2), then we need to check if it's been interrupted and respond appropriately. 
        public void CheckInterruptedUpload()
        {
            if (Linked != -1 && Utils.gameComp.GetLinkedPawn(ThisPawn) != -2)
            {
                // Check to see if the current pawn is no longer connected to the SkyMind network (or is dead).
                if (ThisPawn.Dead || (!ThisPawn.TryGetComp<CompSkyMind>().connected && !Utils.gameComp.GetCloudPawns().Contains(ThisPawn)))
                {
                    HandleInterrupt();
                    return;
                }

                // Check to see if the operation involves a recipient pawn and ensure their status is similarly acceptable if there is one.
                if (recipientPawn != null)
                {
                    if (recipientPawn.Dead || (!recipientPawn.TryGetComp<CompSkyMind>().connected && !Utils.gameComp.GetCloudPawns().Contains(ThisPawn)))
                    {
                        HandleInterrupt();
                        return;
                    }
                }

                // Check to see if there is a functional SkyMind Core if one is required for an operation to continue. One is required for uploading, downloading, or replicating.
                if (Linked >= 4 && Utils.gameComp.GetSkyMindCloudCapacity() == 0)
                {
                    HandleInterrupt();
                    return;
                }
            }
        }

        // Operation tracker. -2 = player surrogate operation, -1 = No operation, 1 = permutation, 2 = duplication, 3 = absorption, 4 = download, 5 = upload, 6 = replication
        private int networkOperationInProgress = -1;
        private Pawn recipientPawn = null;
        private HashSet<Pawn> surrogatePawns = new HashSet<Pawn>();
        private bool controlMode = false;
        public bool isForeign = false;
    }
}