using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ATReforged
{
    public class CompSkyMindCore : ThingComp
    {
        public CompProperties_SkyMindCore Props
        {
            get
            {
                return (CompProperties_SkyMindCore)props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // No need to handle anything upon loading a save - capacity is saved in the GameComponent and we should avoid adding extra capacity.
            if (respawningAfterLoad)
                return;

            // If there is no power supply to this server, it can't be turned on/off normally. Just add it in and handle removing it separately.
            if (parent.GetComp<CompPowerTrader>() == null)
            {
                Utils.gameComp.AddCore(this);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            // Buildings that provide core capacity lose it when they despawn if they are online (whenever something either has no power trader or has an online power trader).
            if (parent is Building && parent.GetComp<CompPowerTrader>()?.PowerOn != false)
            {
                Utils.gameComp.RemoveCore(this);
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            switch (signal)
            {
                case "PowerTurnedOn":
                    Utils.gameComp.AddCore(this);
                    break;
                case "PowerTurnedOff":
                    Utils.gameComp.RemoveCore(this);
                    break;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // No reason to show buttons to check on SkyMind intelligences if none exist.
            if (Utils.gameComp.GetCloudPawns().Count() == 0)
            {
                yield break;
            }

            // Allow all SkyMind intelligences to display their info to the player.
            yield return new Command_Action
            {
                icon = Tex.processInfo,
                defaultLabel = "ATR_CloudPawnInfo".Translate(),
                defaultDesc = "ATR_CloudPawnInfoDesc".Translate(),
                action = delegate ()
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                    foreach (Pawn pawn in Utils.gameComp.GetCloudPawns())
                    {
                        opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                        {
                            Find.WindowStack.Add(new Dialog_InfoCard(pawn));
                        }));
                        opts.SortBy((x) => x.Label);

                        if (opts.Count == 0)
                            return;

                        Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableSources".Translate()));
                    }
                }
            };

            // Allow free SkyMind intelligences to be flushed from the network.
            yield return new Command_Action
            {
                icon = Tex.processRemove,
                defaultLabel = "ATR_RemoveCloudPawn".Translate(),
                defaultDesc = "ATR_RemoveCloudPawnDesc".Translate(),
                action = delegate ()
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                    foreach (Pawn pawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_MindOperation) == null && !pawn.GetComp<CompSkyMindLink>().HasSurrogate()))
                    {
                        opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                        {
                            Find.WindowStack.Add(new Dialog_MessageBox("ATR_RemoveCloudPawnConfirm".Translate(pawn.LabelShortCap), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_RemoveCloudPawn".Translate(), buttonAAction: delegate
                            {
                                Utils.gameComp.PopCloudPawn(pawn);
                                pawn.Kill(null);

                                Messages.Message("ATR_RemoveCloudPawnSuccess".Translate(pawn.LabelShortCap), parent, MessageTypeDefOf.PositiveEvent);

                            }));
                        }));
                        opts.SortBy((x) => x.Label);

                        if (opts.Count == 0)
                            return;

                        Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableSources".Translate()));
                    }
                }
            };

            // Allow replication of a SkyMind networked pawn.
            yield return new Command_Action
            {
                icon = Tex.processReplicate,
                defaultLabel = "ATR_ReplicateCloudPawn".Translate(),
                defaultDesc = "ATR_ReplicateCloudPawnDesc".Translate(),
                action = delegate ()
                {
                    if (Utils.gameComp.GetCloudPawns().Count() > Utils.gameComp.GetSkyMindCloudCapacity())
                    {
                        Messages.Message("ATR_ProcessReplicateFailed".Translate(), parent, MessageTypeDefOf.NegativeEvent);
                    }
                    else
                    {
                        List<FloatMenuOption> opts = new List<FloatMenuOption>();

                        foreach (Pawn pawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_MindOperation) == null && !pawn.GetComp<CompSkyMindLink>().HasSurrogate()))
                        {
                            opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                            {
                                Find.WindowStack.Add(new Dialog_MessageBox("ATR_ReplicateCloudPawnDesc".Translate() + "\n" + "ATR_SkyMindDisconnectionRisk".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_ReplicateCloudPawn".Translate(), buttonAAction: delegate
                                {
                                    pawn.GetComp<CompSkyMindLink>().InitiateConnection(6);
                                }));
                            }));
                            opts.SortBy((x) => x.Label);

                            if (opts.Count == 0)
                                return;
                            Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableSources".Translate()));
                        }
                    }
                }
            };

            // Allow all cloud pawns to use the Skill interface.
            yield return new Command_Action
            {
                icon = Tex.processSkillUp,
                defaultLabel = "ATR_Skills".Translate(),
                defaultDesc = "ATR_SkillsDesc".Translate(),
                action = delegate ()
                {
                    List<FloatMenuOption> cloudPawnOpts = new List<FloatMenuOption>();
                    foreach (Pawn cloudPawn in Utils.gameComp.GetCloudPawns())
                    {
                        cloudPawnOpts.Add(new FloatMenuOption(cloudPawn.LabelShortCap, delegate
                        {
                            Find.WindowStack.Add(new Dialog_SkillUp(cloudPawn));
                        }));
                    }
                    cloudPawnOpts.SortBy((x) => x.Label);

                    if (cloudPawnOpts.Count == 0)
                        return;

                    Find.WindowStack.Add(new FloatMenu(cloudPawnOpts, "ATR_ViableSources".Translate()));
                }
            };

            // No need to check surrogate conditions if settings forbid using them.
            if (!ATReforged_Settings.surrogatesAllowed)
            {
                yield break;
            }

            // Allow connecting a SkyMind pawn to available surrogates.
            yield return new Command_Action
            {
                icon = Tex.ConnectIcon,
                defaultLabel = "ATR_ControlSurrogate".Translate(),
                defaultDesc = "ATR_ControlSurrogateDesc".Translate(),
                action = delegate ()
                {
                    List<FloatMenuOption> cloudPawnOpts = new List<FloatMenuOption>();
                    foreach (Pawn cloudPawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_MindOperation) == null))
                    {
                        cloudPawnOpts.Add(new FloatMenuOption(cloudPawn.LabelShortCap, delegate
                        {
                            List<FloatMenuOption> targetOpts = new List<FloatMenuOption>();
                            foreach (Map map in Find.Maps)
                            {
                                targetOpts.Add(new FloatMenuOption(map.Parent.Label, delegate
                                {
                                    Current.Game.CurrentMap = map;
                                    TargetingParameters targetParameters = new TargetingParameters()
                                    {
                                        canTargetPawns = true,
                                        canTargetBuildings = false,
                                        canTargetAnimals = false,
                                        canTargetMechs = false,
                                        mapObjectTargetsMustBeAutoAttackable = false,
                                        onlyTargetIncapacitatedPawns = true,
                                        validator = delegate (TargetInfo targetInfo)
                                        {
                                            return targetInfo.Thing is Pawn pawn && (pawn.Faction == null || pawn.Faction.IsPlayer) && Utils.IsSurrogate(pawn)
                                                    && pawn.GetComp<CompSkyMind>().Breached == -1 && !pawn.GetComp<CompSkyMindLink>().HasSurrogate();
                                        }
                                    };
                                    Find.Targeter.BeginTargeting(targetParameters, (LocalTargetInfo target) => cloudPawn.GetComp<CompSkyMindLink>().ConnectSurrogate((Pawn)target.Thing));
                                }));
                            }
                            if (targetOpts.Count != 0)
                            {
                                Find.WindowStack.Add(new FloatMenu(targetOpts));
                            }
                        }));
                    }
                    cloudPawnOpts.SortBy((x) => x.Label);

                    if (cloudPawnOpts.Count == 0)
                        return;

                    Find.WindowStack.Add(new FloatMenu(cloudPawnOpts, "ATR_ViableSources".Translate()));
                }
            };

            // Allow disconnecting a particular SkyMind pawn from its surrogates.
            if (Utils.gameComp.GetCloudPawns().Any(pawn => pawn.GetComp<CompSkyMindLink>().HasSurrogate()))
            {
                yield return new Command_Action
                {
                    icon = Tex.DisconnectIcon,
                    defaultLabel = "ATR_DisconnectCloudPawn".Translate(),
                    defaultDesc = "ATR_DisconnectCloudPawnDesc".Translate(),
                    action = delegate ()
                    {
                        List<FloatMenuOption> opts = new List<FloatMenuOption>();

                        foreach (Pawn pawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.GetComp<CompSkyMindLink>().HasSurrogate()))
                        {
                            opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                            {
                                pawn.GetComp<CompSkyMindLink>().DisconnectSurrogates();
                            }));
                            opts.SortBy((x) => x.Label);

                            if (opts.Count == 0)
                                return;
                            Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableSources".Translate()));
                        }
                    }
                };
            }

            // If there are uncontrolled surrogates in a caravan, allow a SkyMind intelligence to control it.
            IEnumerable<Pawn> hostlessCaravanSurrogates = Utils.GetHostlessCaravanSurrogates();
            if (hostlessCaravanSurrogates != null)
            {
                yield return new Command_Action
                {
                    icon = Tex.RecoveryIcon,
                    defaultLabel = "ATR_ControlCaravanSurrogate".Translate(),
                    defaultDesc = "ATR_ControlCaravanSurrogateDesc".Translate(),
                    action = delegate ()
                    {
                        List<FloatMenuOption> cloudPawnOpts = new List<FloatMenuOption>();
                        foreach (Pawn cloudPawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_MindOperation) == null && !pawn.GetComp<CompSkyMindLink>().HasSurrogate()))
                        {
                            cloudPawnOpts.Add(new FloatMenuOption(cloudPawn.LabelShortCap, delegate
                            {
                                List<FloatMenuOption> targetOpts = new List<FloatMenuOption>();
                                foreach (Pawn surrogate in hostlessCaravanSurrogates)
                                {
                                    targetOpts.Add(new FloatMenuOption(surrogate.LabelShortCap, delegate
                                    {
                                        if (!Utils.gameComp.AttemptSkyMindConnection(surrogate))
                                            Messages.Message("ATR_SkyMindConnectionFailed".Translate(), parent, MessageTypeDefOf.NegativeEvent);
                                        else
                                            cloudPawn.GetComp<CompSkyMindLink>().ConnectSurrogate(surrogate);
                                    }));
                                }
                                targetOpts.SortBy((x) => x.Label);

                                if (targetOpts.Count == 0)
                                    return;

                                Find.WindowStack.Add(new FloatMenu(targetOpts, "ATR_ViableTargets".Translate()));
                            }));
                        }
                        cloudPawnOpts.SortBy((x) => x.Label);

                        if (cloudPawnOpts.Count == 0)
                            return;

                        Find.WindowStack.Add(new FloatMenu(cloudPawnOpts, "ATR_ViableSources".Translate()));
                    }
                };
            }

            yield break;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            Dictionary<Pawn, int> linkedPawns = Utils.gameComp.GetAllLinkedPawns();
            if (linkedPawns?.Count > 0)
            {
                HashSet<Pawn> cloudPawns = Utils.gameComp.GetCloudPawns();
                foreach (var linkedPair in linkedPawns)
                {
                    if (cloudPawns.Contains(linkedPair.Key))
                    {
                        ret.AppendLine("ATR_SkyMindCoreOperationInProgress".Translate(linkedPair.Key, (linkedPair.Value - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose()));
                    }
                }
            }
            ret.Append("ATR_CloudIntelligenceSummary".Translate(Utils.gameComp.GetCloudPawns().Count(), Utils.gameComp.GetSkyMindCloudCapacity()));

            return ret.Append(base.CompInspectStringExtra()).ToString();
        }
    }
}