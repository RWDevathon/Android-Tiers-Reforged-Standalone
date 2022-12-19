using System;
using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

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
            else if (Find.DesignatorManager.SelectedDesignator is Designator_AndroidToControl && Utils.IsSurrogate(ThisPawn) && !HasSurrogate())
                avatar = Tex.AvailableSurrogateIcon;

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
                if (Linked == -1 && (Utils.gameComp.GetPointCapacity(ServerType.SkillServer) > 0 || Utils.gameComp.GetPointCapacity(ServerType.HackingServer) > 0))
                {
                    Texture2D tex = Tex.MindAbsorption;
                    yield return new Command_Action
                    {
                        icon = tex,
                        defaultLabel = "ATR_AbsorbExperience".Translate(),
                        defaultDesc = "ATR_AbsorbExperienceDesc".Translate(),
                        action = delegate ()
                        {
                            Find.WindowStack.Add(new Dialog_MessageBox("ATR_AbsorbExperienceConfirm".Translate(parent.LabelShortCap) + "\n" + "ATR_SkyMindDisconnectionRisk".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_AbsorbExperience".Translate(), buttonAAction: delegate
                            {
                                InitiateConnection(3);
                            }));
                        }
                    };
                }

                // Skip all other operations. They are illegal on prisoners.
                yield break;
            }

            // Surrogate only operations.
            if (Utils.IsSurrogate(ThisPawn))
            {
                //Organic surrogates may receive downloads from the pawn they are connected to in the SkyMind network.
                if (!Utils.IsConsideredMechanical(ThisPawn) && HasSurrogate())
                {
                    Pawn controller = surrogatePawns.First();
                    // Ensure only cloud pawn controllers that aren't busy controlling other surrogates or that are in a mind operation already are eligible for downloading from.
                    if (Utils.gameComp.GetCloudPawns().Contains(controller) && controller.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation) == null && !controller.TryGetComp<CompSkyMindLink>().surrogatePawns.Any(surrogate => surrogate != ThisPawn))
                    {
                        yield return new Command_Action
                        {
                            icon = Tex.DownloadFromSkyCloud,
                            defaultLabel = "ATR_DownloadCloudPawn".Translate(),
                            defaultDesc = "ATR_DownloadCloudPawnDesc".Translate(),
                            action = delegate ()
                            {
                                Find.WindowStack.Add(new Dialog_MessageBox("ATR_DownloadCloudPawnConfirm".Translate() + "\n" + "ATR_SkyMindDisconnectionRisk".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_DownloadCloudPawn".Translate(), buttonAAction: delegate
                                {
                                    InitiateConnection(4, controller);
                                }));
                            }
                        };
                    }
                }

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

                // Allow interactions with surrogates of this pawn.
                if (HasSurrogate())
                {
                    // Always allow controllers to disconnect from all pawns.
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

                    // Allow this pawn to do transfers if it is controlling an organic surrogate and isn't controlling multiple surrogates.
                    if (surrogatePawns.Count == 1)
                    {
                        Pawn surrogate = surrogatePawns.First();
                        if (!Utils.IsConsideredMechanical(surrogate))
                        {
                            yield return new Command_Action
                            {
                                icon = Tex.DownloadFromSkyCloud,
                                defaultLabel = "ATR_Transfer".Translate(),
                                defaultDesc = "ATR_TransferDesc".Translate(),
                                action = delegate ()
                                {
                                    Find.WindowStack.Add(new Dialog_MessageBox("ATR_TransferConfirm".Translate(parent.LabelShortCap, "ATR_Surrogate".Translate()) + "\n" + "ATR_SkyMindDisconnectionRisk".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_Transfer".Translate(), buttonAAction: delegate
                                    {
                                        surrogate.TryGetComp<CompSkyMindLink>().InitiateConnection(4, ThisPawn);
                                    }));
                                }
                            };
                        }
                    }
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
                            Find.WindowStack.Add(new Dialog_MessageBox("ATR_PermuteConfirm".Translate(parent.LabelShortCap, colonist.LabelShortCap) + "\n" + "ATR_SkyMindDisconnectionRisk".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_Permute".Translate(), buttonAAction: delegate
                            {
                                InitiateConnection(1, colonist);
                            }));
                        }));
                    }
                    opts.SortBy((x) => x.Label);

                    if (opts.Count == 0)
                        opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                    Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableTargets".Translate()));
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
                        Find.WindowStack.Add(new Dialog_MessageBox("ATR_UploadConfirm".Translate() + "\n" + ("ATR_SkyMindDisconnectionRisk").Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_Upload".Translate(), buttonAAction: delegate
                        {
                            InitiateConnection(5);
                        }));
                    }
                };
            }

            yield break;
        }

        // Controller for the mental operation state of the parent. -2 = surrogate, -1 = No Op, > -1 is some sort of operation. GameComponent handles checks for linked pawns.
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
                // Pawn's operation has ended. Close out appropriate function based on the networkOperation that had been chosen (contained in status).
                if (networkOperationInProgress == -1 && status > -1)
                { 
                    // If the status is resetting because of a failure, notify that a failure occurred. HandleInterrupt takes care of actual negative events.
                    if (ThisPawn.health.hediffSet.hediffs.Any(targetHediff => targetHediff.def == HediffDefOf.ATR_MemoryCorruption || targetHediff.def == RimWorld.HediffDefOf.Dementia))
                    {
                        Find.LetterStack.ReceiveLetter("ATR_OperationFailure".Translate(), "ATR_OperationFailureDesc".Translate(ThisPawn.LabelShortCap), LetterDefOf.NegativeEvent, ThisPawn);
                    }
                    else
                    {
                        HandleSuccess(status);
                    }

                    // All pawns undergo a system reboot upon successful completion of an operation.
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.ATR_LongReboot, ThisPawn, null);
                    hediff.Severity = 1f;
                    ThisPawn.health.AddHediff(hediff, null, null);

                    // Pawns no longer have the mind operation hediff.
                    Hediff target = ThisPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation);
                    if (target != null)
                        ThisPawn.health.RemoveHediff(target);

                    // Recipients lose any MindOperation hediffs as well and also reboot.
                    if (recipientPawn != null)
                    {
                        recipientPawn.health.AddHediff(HediffDefOf.ATR_LongReboot);
                        target = recipientPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation);
                        if (target != null)
                            recipientPawn.health.RemoveHediff(target);
                    }

                    Utils.gameComp.PopNetworkLinkedPawn(ThisPawn);
                }
                // Operation has begun. Stand by until completion or aborted.
                else if (networkOperationInProgress > -1)
                { 
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
                ret.Append("ATR_SkyMindOperationInProgress".Translate((Utils.gameComp.GetLinkedPawn(ThisPawn) - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose()));
            }
            else if (networkOperationInProgress == -2)
            {
                ret.Append("ATR_SurrogateConnected".Translate(surrogatePawns.Count));
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
                Messages.Message("ATR_SurrogateControlled".Translate(ThisPawn.LabelShortCap), ThisPawn, MessageTypeDefOf.PositiveEvent);
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
            Utils.Duplicate(Utils.GetBlank(), ThisPawn, false, false);

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
            Linked = -1;
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

        // An operation has been started. Apply mind operation to this pawn and the recipient pawn (if it exists), and track the current operation in the game component.
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
            // Absorption failure kills the source pawn - they were going to die on success any way.
            if (Linked == 3)
            {
                Utils.gameComp.DisconnectFromSkyMind(ThisPawn);
                Utils.Duplicate(Utils.GetBlank(), ThisPawn, true, false);
                ThisPawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 99999f, 999f, -1f, null, ThisPawn.health.hediffSet.GetBrain()));
                // If they're somehow not dead from that, make them dead for real.
                if (!ThisPawn.Dead)
                {
                    ThisPawn.Kill(null);
                }
                return;
            }

            // Otherwise, corrupt the source and reicipient (if it exists) pawns and delink them.
            ApplyCorruption(ThisPawn);
            if (recipientPawn != null)
            {
                ApplyCorruption(recipientPawn);
                recipientPawn.TryGetComp<CompSkyMindLink>().Linked = -1;
            }
            Utils.gameComp.PopNetworkLinkedPawn(ThisPawn);
            Linked = -1;
        }

        // Apply the correct effects upon successfully completing the SkyMind operation based on the status (parameter as Linked is changed to -1 just beforehand).
        public void HandleSuccess(int status)
        {
            Hediff target;
            // If there is a recipient pawn, it was a permutation, duplication, or download. Handle appropriately.
            if (recipientPawn != null)
            {
                // Permutation, swap the pawn's minds.
                if (status == 1)
                {
                    Utils.PermutePawn(ThisPawn, recipientPawn);
                }
                // Duplication, insert a copy of the current pawn into the recipient - an organic surrogate that will now become a fully fledged individual again.
                else if (status == 2)
                {
                    // Disconnect the surrogate to sever the connection, then duplicate this pawn into the surrogate.
                    Utils.gameComp.DisconnectFromSkyMind(recipientPawn);
                    Utils.Duplicate(ThisPawn, recipientPawn, false, false);
                    // Duplication may only occur via operation on organic surrogates. Remove the receiver (burns out), remove no host hediff.
                    target = recipientPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_SkyMindReceiver);
                    if (target != null)
                        recipientPawn.health.RemoveHediff(target);
                    target = recipientPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_NoController);
                    if (target != null)
                        recipientPawn.health.RemoveHediff(target);
                }
                // Download, insert of a copy of the recipient pawn into the current pawn. After, destroy the recipient's intelligence.
                // For androids, this means becoming a blank. For humans, a wild person. For SkyMind intelligences, ceasing to exist.
                else if (status == 4)
                {
                    // Organic pawns that have been downloaded into should lose some hediffs and disconnect from the network.
                    if (!Utils.IsConsideredMechanicalAndroid(ThisPawn))
                    {
                        Utils.gameComp.DisconnectFromSkyMind(ThisPawn);
                        target = ThisPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_SkyMindReceiver);
                        if (target != null)
                            ThisPawn.health.RemoveHediff(target);
                        target = ThisPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_NoController);
                        if (target != null)
                            ThisPawn.health.RemoveHediff(target);
                    }

                    Utils.Duplicate(recipientPawn, ThisPawn, false, false);
                    Utils.gameComp.DisconnectFromSkyMind(recipientPawn);
                    Utils.KillPawnIntelligence(recipientPawn);
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
                Utils.Duplicate(Utils.GetBlank(), ThisPawn, true, false);
                ThisPawn.Kill(null);
            }

            // Upload moves the pawn to the SkyMind network (despawns and puts into storage).
            if (status == 5)
            {
                // Add the pawn to storage and suspend any tick-checks it performs.
                Utils.gameComp.PushCloudPawn(ThisPawn);
                Current.Game.tickManager.DeRegisterAllTickabilityFor(ThisPawn);

                // Upon completion, we need to spawn a copy of the pawn to take their physical place as the original pawn despawns "into" the SkyMind Core. 
                Pawn replacement = Utils.SpawnCopy(ThisPawn, ATReforged_Settings.uploadingToSkyMindKills);
                // If in the settings, uploading is set to Permakill, find the new pawn copy's brain and mercilessly destroy it so it can't be revived. Ensure no one cares about this.
                if (ATReforged_Settings.uploadingToSkyMindPermaKills)
                {
                    replacement.SetFactionDirect(null);
                    replacement.relations.ClearAllRelations();
                    replacement.ideo?.SetIdeo(null);
                    replacement.TakeDamage(new DamageInfo(DamageDefOf.Burn, 99999f, 999f, -1f, null, replacement.health.hediffSet.GetBrain()));
                }

                // The pawn does not need to be connected to the SkyMind directly now, and should disappear.
                Utils.gameComp.DisconnectFromSkyMind(ThisPawn);
                ThisPawn.DeSpawn();
                ThisPawn.ownership.UnclaimAll();
            }

            // Replication simply creates a new SkyMind intelligence duplicated from another.
            if (status == 6)
            {
                // Generate the clone. 
                PawnGenerationRequest request = new PawnGenerationRequest(ThisPawn.kindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, fixedBiologicalAge: ThisPawn.ageTracker.AgeBiologicalYearsFloat, fixedChronologicalAge: ThisPawn.ageTracker.AgeChronologicalYearsFloat, fixedGender: ThisPawn.gender);
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
            if (Linked > -1 && Utils.gameComp.GetLinkedPawn(ThisPawn) != -2)
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
                    if (recipientPawn.Dead || (!recipientPawn.TryGetComp<CompSkyMind>().connected && !Utils.gameComp.GetCloudPawns().Contains(recipientPawn)))
                    {
                        HandleInterrupt();
                        return;
                    }
                }

                // Check to see if there is a functional SkyMind Core if one is required for an operation to continue. One is required for uploading, or replicating.
                if (Linked >= 5 && Utils.gameComp.GetSkyMindCloudCapacity() == 0)
                {
                    HandleInterrupt();
                    return;
                }
            }
        }

        // A public method for other classes to initiate SkyMind operations. It will fail and do nothing if this pawn is already preoccupied.
        public void InitiateConnection(int operationType, Pawn targetRecipient = null)
        {
            if (Linked > -1)
            {
                Log.Warning("[ATR] Something attempted to initiate a connection for " + ThisPawn + " while it was busy! Command was ignored.");
                return;
            }
            else if (operationType == -2)
            {
                Log.Warning("[ATR] Surrogate connections can not be established directly with the InitiateConnection function. Use ConnectSurrogate instead.");
            }

            recipientPawn = targetRecipient;
            Linked = operationType;
        }

        // Operation tracker. -2 = player surrogate operation, -1 = No operation, 1 = permutation, 2 = duplication, 3 = absorption, 4 = download, 5 = upload, 6 = replication
        private int networkOperationInProgress = -1;

        // Tracker for the recipient pawn of a mind operation that requires two linked units.
        private Pawn recipientPawn = null;

        // Tracker for all surrogate pawns. If a pawn is a surrogate, it will have exactly one link - to its host. If it is a controller, it has links to all surrogates.
        private HashSet<Pawn> surrogatePawns = new HashSet<Pawn>();

        // Tracker for if this pawn is in control mode (allowing control of surrogates).
        private bool controlMode = false;

        // Tracker for whether this pawn is not a player surrogate. Foreign surrogates do not have links to their controllers and are very limited in what they can do.
        public bool isForeign = false;
    }
}