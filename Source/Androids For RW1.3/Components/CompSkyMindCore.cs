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
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            // Check to see if the core actually should disappear or not. Buildings always will, but despawned, living Pawns still qualify as cores.
            if (parent is Building || ((Pawn)parent).Dead)
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
            // Don't show a button to interact with SkyMind intelligences on a broken core.
            if (parent is Building)
            {
                if (parent.Destroyed || !parent.TryGetComp<CompPowerTrader>().PowerOn)
                yield break;
            }

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

                        Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableCloudPawns".Translate()));
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

                    foreach (Pawn pawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation) == null && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()))
                    {
                        opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                        {
                            Find.WindowStack.Add(new Dialog_Msg("ATR_RemoveCloudPawn".Translate(), "ATR_RemoveCloudPawnConfirm".Translate(pawn.LabelShortCap), delegate
                            {
                                Utils.gameComp.PopCloudPawn(pawn);
                                pawn.Kill(null);

                                Messages.Message("ATR_RemoveCloudPawnSuccess".Translate(pawn.LabelShortCap), parent, MessageTypeDefOf.PositiveEvent);

                            }));
                        }));
                        opts.SortBy((x) => x.Label);

                        if (opts.Count == 0)
                            return;

                        Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableCloudPawns".Translate()));
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

                        foreach (Pawn pawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation) == null && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()))
                        {
                            opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                            {
                                Find.WindowStack.Add(new Dialog_Msg("ATR_ReplicateCloudPawn".Translate(), "ATR_ReplicateCloudPawnDesc".Translate() + "\n" + ("ATR_SkyMindDisconnectionRisk").Translate(), delegate
                                {
                                    pawn.TryGetComp<CompSkyMindLink>().Linked = 6;
                                    Utils.gameComp.PushNetworkLinkedPawn(pawn, Find.TickManager.TicksGame + ATReforged_Settings.timeToCompleteSkyMindOperations * 2500);
                                }));
                            }));
                            opts.SortBy((x) => x.Label);

                            if (opts.Count == 0)
                                return;
                            Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableCloudPawns".Translate()));
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

                    Find.WindowStack.Add(new FloatMenu(cloudPawnOpts, "ATR_ViableCloudPawns".Translate()));
                }
            };

            // Allow connecting free SkyMind Intelligences to surrogates.
            yield return new Command_Action
            {
                icon = Tex.ConnectIcon,
                defaultLabel = "ATR_ControlSurrogate".Translate(),
                defaultDesc = "ATR_ControlSurrogateDesc".Translate(),
                action = delegate ()
                {
                    List<FloatMenuOption> cloudPawnOpts = new List<FloatMenuOption>();
                    foreach (Pawn cloudPawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation) == null && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()))
                    {
                        cloudPawnOpts.Add(new FloatMenuOption(cloudPawn.LabelShortCap, delegate
                        {
                            List<FloatMenuOption> targetOpts = new List<FloatMenuOption>();
                            foreach (Map map in Find.Maps)
                            {
                                targetOpts.Add(new FloatMenuOption(map.Parent.Label, delegate
                                {
                                    Current.Game.CurrentMap = map;
                                    Designator_AndroidToControl target = new Designator_AndroidToControl(cloudPawn);
                                    Find.DesignatorManager.Select(target);

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

                    Find.WindowStack.Add(new FloatMenu(cloudPawnOpts, "ATR_ViableCloudPawns".Translate()));
                }
            };

            IEnumerable<Pawn> hostlessCaravanSurrogates = Utils.GetHostlessCaravanSurrogates();
            // If there are uncontrolled surrogates in a caravan, allow a SkyMind intelligence to control it.
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
                        foreach (Pawn cloudPawn in Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation) == null && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()))
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
                                            cloudPawn.TryGetComp<CompSkyMindLink>().ConnectSurrogate(surrogate);
                                    }));
                                }
                                targetOpts.SortBy((x) => x.Label);

                                if (targetOpts.Count == 0)
                                    return;

                                Find.WindowStack.Add(new FloatMenu(targetOpts, "ATR_ViableTargetPawns".Translate()));
                            }));
                        }
                        cloudPawnOpts.SortBy((x) => x.Label);

                        if (cloudPawnOpts.Count == 0)
                            return;

                        Find.WindowStack.Add(new FloatMenu(cloudPawnOpts, "ATR_ViableCloudPawns".Translate()));
                    }
                };
            }

            yield break;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            ret.AppendLine("ATR_CloudIntelligenceSummary".Translate(Utils.gameComp.GetCloudPawns().Count(), Utils.gameComp.GetSkyMindCloudCapacity()));
            ret.Append("ATR_AssistingIntelligenceSummary".Translate(Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_MindOperation) == null && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()).Count()));

            return ret.Append(base.CompInspectStringExtra()).ToString();
        }
    }
}