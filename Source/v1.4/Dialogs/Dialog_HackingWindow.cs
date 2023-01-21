using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ATReforged
{
    public class Dialog_HackingWindow : Window
    {
        public static Vector2 scrollPosition = Vector2.zero;

        public Dialog_HackingWindow()
        {
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnAccept = true;
            closeOnClickedOutside = true;
            cachedHackPenalty = Utils.gameComp.hackCostTimePenalty;
            ResetCostModifiers();
        }

        float cachedScrollHeight = 0;
        int cachedHackPenalty;
        float availableHackingPoints;

        readonly static float guidanceCostBase = 400f;
        readonly static float guidanceSuccessChanceBase = 0.8f;
        float guidanceCostModifier;

        readonly static float propagandaCostBase = 500f;
        readonly static float propagandaSuccessChanceBase = 0.75f;
        float propagandaCostModifier;

        readonly static float espionageCostBase = 600f;
        readonly static float espionageSuccessChanceBase = 0.6f;
        float espionageCostModifier;

        readonly static float disruptorCostBase = 600f;
        readonly static float disruptorSuccessChanceBase = 0.7f;
        float disruptorCostModifier;

        public override void DoWindowContents(Rect inRect)
        {
            Color colorSave = GUI.color;
            TextAnchor anchorSave = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Initialize header, footer, and content boxes.
            var headerRect = inRect.TopPartPixels(40);
            var footerRect = inRect.BottomPartPixels(90);
            var mainRect = new Rect(inRect);
            mainRect.y += 40;
            mainRect.height -= 130;

            // Display header content image and pawn information.
            Listing_Standard prelist = new Listing_Standard();
            prelist.Begin(headerRect);

            prelist.Label("ATR_HackInterfaceTitle".Translate());
            prelist.GapLine();

            prelist.End();

            // Ensure main content has enough space to display everything using a scrollbox.
            bool needToScroll = cachedScrollHeight > mainRect.height;
            var viewRect = new Rect(mainRect);
            if (needToScroll)
            {
                viewRect.width -= 20f;
                viewRect.height = cachedScrollHeight;
                Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            }

            Listing_Standard listingStandard = new Listing_Standard
            {
                maxOneColumn = true
            };
            listingStandard.Begin(viewRect);

            float maxWidth = listingStandard.ColumnWidth;

            availableHackingPoints = Utils.gameComp.GetPoints(ServerType.HackingServer);

            // Guidance Hack operation
            HackSelector(listingStandard, maxWidth, "ATR_GuidanceHack".Translate(), ref guidanceCostModifier, guidanceCostBase, OperationSuccessChance(guidanceSuccessChanceBase, guidanceCostBase, guidanceCostModifier), "ATR_GuidanceHackDesc".Translate(), delegate
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();

                // Get a list of all other factions technologically capable of using GPS - they are the list of viable targets. We don't care about their relationship to the player.
                foreach (Faction targetFaction in Find.FactionManager.GetFactions(minTechLevel: TechLevel.Industrial))
                {
                    opts.Add(new FloatMenuOption(targetFaction.Name, delegate
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox("ATR_GuidanceHackDesc".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_GuidanceHack".Translate(), buttonAAction: delegate
                        {
                            // A faction has been selected for targetting. Check if the hack was successful. Handle failure and success separately.
                            if (!Rand.Chance(OperationSuccessChance(guidanceSuccessChanceBase, guidanceCostBase, guidanceCostModifier)))
                            { // Failed
                                HandleOperationFailure(targetFaction);
                            }
                            else // Succeeded
                            {
                                // Try to generate a random pod or orbital event.
                                float randPick = Rand.Range(0.0f, 1.0f);
                                FiringIncident incident;
                                // Resource pod drop
                                if (randPick < 0.4f)
                                {
                                    incident = new FiringIncident
                                    {
                                        def = ATR_IncidentDefOf.ResourcePodCrash,
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, Find.CurrentMap)
                                    };
                                }
                                // Orbital trader arrival
                                else if (randPick < 0.6f)
                                {
                                    incident = new FiringIncident
                                    {
                                        def = IncidentDefOf.OrbitalTraderArrival,
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.OrbitalVisitor, Find.CurrentMap)
                                    };
                                }
                                // Refugee pod drop
                                else if (randPick < 0.75f)
                                {
                                    incident = new FiringIncident
                                    {
                                        def = ATR_IncidentDefOf.RefugeePodCrash,
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, Find.CurrentMap)
                                    };
                                }
                                // Ship chunk drop
                                else if (randPick < 0.9f)
                                {
                                    incident = new FiringIncident
                                    { 
                                        def = IncidentDefOf.ShipChunkDrop, 
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ShipChunkDrop, Find.CurrentMap) 
                                    };
                                }
                                // Drop-pod raid
                                else if (randPick < 0.93f)
                                {
                                    incident = new FiringIncident
                                    { 
                                        def = IncidentDefOf.RaidEnemy, 
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap) 
                                    };
                                    incident.parms.points /= 2;
                                    incident.parms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
                                    incident.parms.raidNeverFleeIndividual = true;
                                }
                                // Mech-cluster drop
                                else if (randPick < 0.95f)
                                {
                                    incident = new FiringIncident
                                    { 
                                        def = IncidentDefOf.MechCluster, 
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap) 
                                    };
                                    incident.parms.points /= 2;
                                }
                                // Psychic Ship-part drop
                                else if (randPick < 0.97f)
                                {
                                    incident = new FiringIncident
                                    {
                                        def = ATR_IncidentDefOf.PsychicEmanatorShipPartCrash,
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap)
                                    };
                                    incident.parms.points /= 2;
                                }
                                // Defoliator Ship-part drop
                                else
                                {
                                    incident = new FiringIncident
                                    {
                                        def = ATR_IncidentDefOf.DefoliatorShipPartCrash,
                                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap)
                                    };
                                    incident.parms.points /= 2;
                                }

                                // Ensure the incident is forced to avoid situations where it is normally forbidden.
                                incident.parms.forced = true;
                                if (!Find.Storyteller.TryFire(incident))
                                {
                                    Messages.Message("ATR_NoArrival".Translate(), MessageTypeDefOf.NeutralEvent, false);
                                }
                            }
                            cachedHackPenalty += (int)guidanceCostBase;
                            Utils.gameComp.hackCostTimePenalty += (int)guidanceCostBase;
                            Utils.gameComp.ChangeServerPoints(-guidanceCostModifier, ServerType.HackingServer);
                            ResetCostModifiers();
                        }));
                    }));
                    opts.SortBy((x) => x.Label);

                    if (opts.Count == 0)
                    {
                        opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                        Find.WindowStack.Add(new FloatMenu(opts));
                        return;
                    }

                    Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableTargets".Translate()));
                }
            });

            // Propaganda Hack operation
            HackSelector(listingStandard, maxWidth, "ATR_PropagandaHack".Translate(), ref propagandaCostModifier, propagandaCostBase, OperationSuccessChance(propagandaSuccessChanceBase, propagandaCostBase, propagandaCostModifier), "ATR_PropagandaHackDesc".Translate(), delegate
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();

                // Get a list of all other non-permanently-hostile factions technologically capable of possessing long-distance communications - they are the list of viable targets.
                foreach (Faction targetFaction in Find.FactionManager.GetFactions(minTechLevel: TechLevel.Industrial).Where(faction => faction.HasGoodwill && faction.GoodwillWith(Faction.OfPlayer) > -100))
                {
                    opts.Add(new FloatMenuOption(targetFaction.Name, delegate
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox("ATR_PropagandaHackDesc".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_PropagandaHack".Translate(), buttonAAction: delegate
                        {
                            // A faction has been selected for targetting. Check if the hack was successful. Handle failure and success separately.
                            if (!Rand.Chance(OperationSuccessChance(propagandaSuccessChanceBase, propagandaCostBase, propagandaCostModifier)))
                            { // Failed
                                HandleOperationFailure(targetFaction);
                            }
                            else // Succeeded
                            {
                                // Try to increase good will with the faction. It will send a message of how much it improved itself.
                                targetFaction.TryAffectGoodwillWith(Faction.OfPlayer, Rand.Range(5, 15));
                            }
                            cachedHackPenalty += (int)propagandaCostBase;
                            Utils.gameComp.hackCostTimePenalty += (int)propagandaCostBase;
                            Utils.gameComp.ChangeServerPoints(-propagandaCostModifier, ServerType.HackingServer);
                            ResetCostModifiers();
                        }));
                    }));
                    opts.SortBy((x) => x.Label);

                    if (opts.Count == 0)
                    {
                        opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                        Find.WindowStack.Add(new FloatMenu(opts));
                        return;
                    }

                    Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableTargets".Translate()));
                }
            });

            // Espionage Hack operation
            HackSelector(listingStandard, maxWidth, "ATR_EspionageHack".Translate(), ref espionageCostModifier, espionageCostBase, OperationSuccessChance(espionageSuccessChanceBase, espionageCostBase, espionageCostModifier), "ATR_EspionageHackDesc".Translate(), delegate
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();

                // Get a list of all other factions technologically capable of being listened in on - they are the list of viable targets.
                foreach (Faction targetFaction in Find.FactionManager.GetFactions(minTechLevel: TechLevel.Industrial))
                {
                    opts.Add(new FloatMenuOption(targetFaction.Name, delegate
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox("ATR_EspionageHackDesc".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_EspionageHack".Translate(), buttonAAction: delegate
                        {
                            // A faction has been selected for targetting. Check if the hack was successful. Handle failure and success separately.
                            if (!Rand.Chance(OperationSuccessChance(espionageSuccessChanceBase, espionageCostBase, espionageCostModifier)))
                            { // Failed
                                HandleOperationFailure(targetFaction);
                            }
                            else // Succeeded
                            {
                                // Get a random opportunity site and generate it.
                                Quest quest;
                                float randPick = Rand.Range(0.0f, 1.0f);
                                // Generate a ancient complex loot quest.
                                if (randPick <= 0.15f && ModsConfig.IdeologyActive)
                                {
                                    quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_AncientComplex, StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap));
                                    if (!quest.hidden && QuestScriptDefOf.OpportunitySite_AncientComplex.sendAvailableLetter)
                                    {
                                        QuestUtility.SendLetterQuestAvailable(quest);
                                    }
                                }
                                // Generate a work site quest.
                                else if (randPick <= 0.45f && ModsConfig.IdeologyActive)
                                {
                                    quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_WorkSite, 2 * StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap));
                                    if (!quest.hidden && QuestScriptDefOf.OpportunitySite_WorkSite.sendAvailableLetter)
                                    {
                                        QuestUtility.SendLetterQuestAvailable(quest);
                                    }
                                }
                                // Generate an item stash quest.
                                else
                                {
                                    quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_ItemStash, 3 * StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap));
                                    if (!quest.hidden && QuestScriptDefOf.OpportunitySite_ItemStash.sendAvailableLetter)
                                    {
                                        QuestUtility.SendLetterQuestAvailable(quest);
                                    }
                                }

                            }
                            cachedHackPenalty += (int)espionageCostBase;
                            Utils.gameComp.hackCostTimePenalty += (int)espionageCostBase;
                            Utils.gameComp.ChangeServerPoints(-espionageCostModifier, ServerType.HackingServer);
                            ResetCostModifiers();
                        }));
                    }));
                    opts.SortBy((x) => x.Label);

                    if (opts.Count == 0)
                    {
                        opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                        Find.WindowStack.Add(new FloatMenu(opts));
                        return;
                    }

                    Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableTargets".Translate()));
                }
            });

            // Disruptor Hack operation
            HackSelector(listingStandard, maxWidth, "ATR_DisruptorHack".Translate(), ref disruptorCostModifier, disruptorCostBase, OperationSuccessChance(disruptorSuccessChanceBase, disruptorCostBase, disruptorCostModifier), "ATR_DisruptorHackDesc".Translate(), delegate
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();

                // Get a list of all other hostile factions technologically capable of using the SkyMind network
                foreach (Faction targetFaction in Find.FactionManager.GetFactions(minTechLevel: TechLevel.Industrial).Where(faction => faction.HostileTo(Faction.OfPlayer) && Utils.FactionCanUseSkyMind(faction.def)))
                {
                    IEnumerable<Pawn> targetPawns = Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(targetFaction).Where(pawn => Utils.HasCloudCapableImplant(pawn));
                    if (targetPawns.Any())
                    {
                        opts.Add(new FloatMenuOption(targetFaction.Name, delegate
                        {
                            Find.WindowStack.Add(new Dialog_MessageBox("ATR_DisruptorHackDesc".Translate(), "Confirm".Translate(), buttonBText: "Cancel".Translate(), title: "ATR_DisruptorHack".Translate(), buttonAAction: delegate
                            {
                                // A faction has been selected for targetting. Check if the hack was successful. Handle failure and success separately.
                                if (!Rand.Chance(OperationSuccessChance(disruptorSuccessChanceBase, disruptorCostBase, disruptorCostModifier)))
                                { // Failed
                                    HandleOperationFailure(targetFaction);
                                }
                                else // Succeeded
                                {
                                    // Apply the DDOS hediff to all hostile SkyMind-capable pawns of this faction.
                                    foreach (Pawn pawn in targetPawns)
                                    {
                                        Hediff ddosHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_RecoveringFromDDOS, pawn, pawn.health.hediffSet.GetBrain());
                                        ddosHediff.Severity = 1;
                                        pawn.health.AddHediff(ddosHediff, pawn.health.hediffSet.GetBrain());
                                    }

                                    Messages.Message("ATR_DisruptorHackSuccess".Translate(), MessageTypeDefOf.PositiveEvent, false);
                                }
                                cachedHackPenalty += (int)disruptorCostBase;
                                Utils.gameComp.hackCostTimePenalty += (int)disruptorCostBase;
                                Utils.gameComp.ChangeServerPoints(-disruptorCostModifier, ServerType.HackingServer);
                                ResetCostModifiers();
                            }));
                        }));
                        opts.SortBy((x) => x.Label);

                        if (opts.Count == 0)
                        {
                            opts.Add(new FloatMenuOption("ATR_NoAvailableTarget".Translate(), null));
                            Find.WindowStack.Add(new FloatMenu(opts));
                            return;
                        }

                        Find.WindowStack.Add(new FloatMenu(opts, "ATR_ViableTargets".Translate()));
                    }
                }
            });


            cachedScrollHeight = listingStandard.CurHeight;
            listingStandard.End();

            if (needToScroll)
            {
                Widgets.EndScrollView();
            }

            // Display the available points and their usage note.
            Listing_Standard postlist = new Listing_Standard();
            postlist.Begin(footerRect);

            postlist.Label("ATR_AvailableHackingPoints".Translate(availableHackingPoints, cachedHackPenalty));
            postlist.Label("ATR_AvailableHackingPointsDesc".Translate());

            postlist.End();

            GUI.color = colorSave;
            Text.Anchor = anchorSave;
        }

        protected void HackSelector(Listing_Standard listingStandard, float sectionWidth, string title, ref float costModifier, float baseCost, float successChance, string operationDescription, Action operation)
        {
            // If the operation can be afforded, display the cost modification slider and the activation button.
            if (baseCost + cachedHackPenalty <= availableHackingPoints)
            {
                // Cost slider
                listingStandard.SliderLabeled(title, ref costModifier, baseCost + cachedHackPenalty, availableHackingPoints);

                // Activation button displaying success chance.
                if (listingStandard.ButtonText("ATR_SuccessChance".Translate(successChance * 100), operationDescription))
                {
                    operation.Invoke();
                }

            }
            else
                listingStandard.Label("ATR_InsufficientPoints".Translate(baseCost + cachedHackPenalty));

            listingStandard.GapLine();
        }

        // On failure, check if there is a retaliation event and handle appropriately.
        protected void HandleOperationFailure(Faction faction)
        {
            // Retaliation events come in several flavors and range from inconvenient to dangerous.
            if (Rand.Chance(ATReforged_Settings.retaliationChanceOnFailure))
            {
                float retaliationPick = Rand.Range(0.0f, 1.0f);
                // Raid
                if (retaliationPick < 0.5f && faction.HostileTo(Faction.OfPlayer))
                {
                    // Try to generate a raid with normal raid points.
                    FiringIncident incident = new FiringIncident
                    {
                        def = IncidentDefOf.RaidEnemy,
                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap)
                    };
                    if (!Find.Storyteller.TryFire(incident))
                    {
                        // Incident was unable to fire, try a counterhack instead. If none can fire, then try to affect goodwill instead.
                        incident = new FiringIncident
                        {
                            def = ATR_IncidentDefOf.ATR_HackingIncident,
                            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, Find.CurrentMap)
                        };
                        if (!Find.Storyteller.TryFire(incident))
                        {
                            // Incident was unable to fire, try to damage goodwill instead.
                            RelationshipRetaliation(faction);
                        }
                    }
                }
                // Problem Causer site
                else if (retaliationPick < 0.75f && faction.HostileTo(Faction.OfPlayer) && ModLister.RoyaltyInstalled)
                {
                    Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(ATR_QuestScriptDefOf.ProblemCauser, StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap));
                    if (ATR_QuestScriptDefOf.ProblemCauser.sendAvailableLetter)
                    {
                        QuestUtility.SendLetterQuestAvailable(quest);
                    }
                    else
                    {
                        // Quest was unable to fire, try a counterhack instead. If none can fire, then try to affect goodwill instead.
                        FiringIncident incident = new FiringIncident
                        {
                            def = ATR_IncidentDefOf.ATR_HackingIncident,
                            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, Find.CurrentMap)
                        };
                        if (!Find.Storyteller.TryFire(incident))
                        {
                            // Incident was unable to fire, try to damage goodwill instead.
                            RelationshipRetaliation(faction);
                        }
                    }
                }
                // Counterhack
                else if (retaliationPick < 0.5f || (faction.HostileTo(Faction.OfPlayer) && retaliationPick < 0.9f))
                {
                    // Try to execute a random hack incident. If none can fire, then try to affect goodwill instead.
                    FiringIncident incident = new FiringIncident
                    {
                        def = ATR_IncidentDefOf.ATR_HackingIncident,
                        parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, Find.CurrentMap)
                    };
                    if (!Find.Storyteller.TryFire(incident))
                    {
                        // Incident was unable to fire, try to damage goodwill instead.
                        RelationshipRetaliation(faction);
                    }
                }
                // Reputation loss
                else
                {
                    // Try to damage goodwill.
                    RelationshipRetaliation(faction);
                }
            }
            // Minor failures send a message notifying the player the operation failed without a major incident.
            else
            {
                Messages.Message("ATR_HackOperationFailed".Translate(), MessageTypeDefOf.NegativeEvent, false);
            }
        }

        // Reset costModifiers to the baseCost + cachedHackPenalty
        private void ResetCostModifiers()
        {
            guidanceCostModifier = guidanceCostBase + cachedHackPenalty;
            propagandaCostModifier = propagandaCostBase + cachedHackPenalty;
            espionageCostModifier = espionageCostBase + cachedHackPenalty;
            disruptorCostModifier = disruptorCostBase + cachedHackPenalty;
        }

        // Generate a relationship retaliation, which can occur as a fallback for other retaliations failing.
        private void RelationshipRetaliation(Faction faction)
        {
            // Try to damage goodwill.
            int goodwillImpact = Rand.Range(-5, -15);
            if (faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwillImpact))
            {
                Find.LetterStack.ReceiveLetter("ATR_RelationshipRetaliation".Translate(), "ATR_RelationshipRetaliationDesc".Translate(goodwillImpact), LetterDefOf.NegativeEvent);
            }
        }
        
        // Generate and return the success chance of an operation.
        protected float OperationSuccessChance(float baseSuccessChance, float baseCost, float costModifier)
        {
            return Mathf.Clamp(baseSuccessChance * (costModifier / (baseCost + cachedHackPenalty)) * RecentHackSuccessPenalty.Evaluate(cachedHackPenalty), ATReforged_Settings.minHackSuccessChance, ATReforged_Settings.maxHackSuccessChance);
        }

        // Recent successes in hacking decreases the likelihood of hacking success as other factions raise short-term security measures.
        private static readonly SimpleCurve RecentHackSuccessPenalty = new SimpleCurve
        {
            new CurvePoint(0f, 1f),
            new CurvePoint(3000f, 0.8f),
            new CurvePoint(10000f, 0.6f)
        };
    }
}