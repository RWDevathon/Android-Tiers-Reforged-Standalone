using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;

namespace ATReforged
{
    public class PawnGroupMakerUtility_Patch
    {
        // Handle generation of groups of pawns so that foreign factions may use surrogates. Random selected pawns of the group will be controlled surrogates.
        [HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns")]
        public class GeneratePawns_Patch
        {
            [HarmonyPostfix]
            public static void Listener(PawnGroupMakerParms parms, bool warnOnZeroResults, ref IEnumerable<Pawn> __result)
            {
                // Generated mechanical pawns in proper groups will always receive the Stasis Hediff to reduce their power consumption significantly.
                foreach (Pawn member in __result)
                {
                    Hediff stasisHediff = member.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_StasisPill);
                    if (Utils.IsConsideredMechanical(member) && stasisHediff == null)
                    {
                        member.health.AddHediff(HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_StasisPill, member));
                    }
                    else if (stasisHediff != null)
                    {
                        stasisHediff.Severity = 1f;
                    }
                }

                // If settings disable this faction from using surrogates (or all surrogates are banned entirely), then there is no work to do here. Allow default generation to proceed.
                if (!ATReforged_Settings.surrogatesAllowed || !ATReforged_Settings.otherFactionsAllowedSurrogates || parms.faction == null)
                {
                    return;
                }

                // Only android reserved factions may use surrogates.
                if (!Utils.ReservedAndroidFactions.Contains(parms.faction.def.defName))
                {
                    return;
                }

                try
                {
                    List<Pawn> surrogateCandidates = new List<Pawn>();

                    foreach (Pawn pawn in __result)
                    {
                        // Count all non-trader pawns with humanlike intelligence that are organics with the proper setting or androids with the proper setting. Don't take pawns that have relations.
                        if (pawn.def.race != null && pawn.def.race.Humanlike && pawn.trader == null && pawn.TraderKind == null && !pawn.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
                        {
                            surrogateCandidates.Add(pawn);
                        }
                    }

                    // Skip groups that are too small
                    if (surrogateCandidates.Count <= ATReforged_Settings.minGroupSizeForSurrogates)
                    {
                        return;
                    }

                    // Determine how many surrogates are taking the place of candidates
                    int surCount = (int)(surrogateCandidates.Count * Rand.Range(ATReforged_Settings.minSurrogatePercentagePerLegalGroup, ATReforged_Settings.maxSurrogatePercentagePerLegalGroup));

                    IEnumerable<Pawn> selectedPawns = surrogateCandidates.TakeRandom(surCount);

                    // Set the selected pawn to control itself, as foreign surrogates do not actually have separate pawns to control them.
                    foreach (Pawn selectedPawn in selectedPawns)
                    {
                        // Ensure the pawn has some sort of SkyMind enabling implant if they can not use the SkyMind network inherently.
                        ATR_MechTweaker androidExtension = selectedPawn.def.GetModExtension<ATR_MechTweaker>();
                        if (androidExtension?.canInherentlyUseSkyMind ?? false)
                        {
                            // Androids use receiver cores as their receivers.
                            if (androidExtension.needsCoreAsAndroid && Utils.IsConsideredMechanicalAndroid(selectedPawn))
                            {
                                Hediff target = selectedPawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_AutonomousCore);
                                if (target != null)
                                {
                                    selectedPawn.health.RemoveHediff(target);
                                }
                                target = selectedPawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_IsolatedCore);
                                if (target != null)
                                {
                                    selectedPawn.health.RemoveHediff(target);
                                }
                                selectedPawn.health.AddHediff(ATR_HediffDefOf.ATR_ReceiverCore, selectedPawn.health.hediffSet.GetBrain());
                            }
                            // Organics that do not specify a receiver implant use the SkyMind receiver.
                            else if (!Utils.IsConsideredMechanical(selectedPawn))
                            {
                                selectedPawn.health.AddHediff(ATR_HediffDefOf.ATR_SkyMindReceiver, selectedPawn.health.hediffSet.GetBrain());
                            }
                        }

                        // Connect the chosenCandidate to the surrogate as the controller. It is an external controller.
                        selectedPawn.GetComp<CompSkyMindLink>().ConnectSurrogate(selectedPawn, true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("[ATR] Failed to apply surrogates when creating a Pawn Group. Unknown consequences may occur." + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}