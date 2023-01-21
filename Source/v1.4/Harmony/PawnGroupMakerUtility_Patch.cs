using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace ATReforged
{
    internal class PawnGroupMakerUtility_Patch

    {
        // Handle generation of groups of pawns so that foreign factions may use surrogates. These surrogates replace selected pawns of the group.
        [HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns")]
        public class GeneratePawns_Patch
        {
            [HarmonyPostfix]
            public static void Listener(PawnGroupMakerParms parms, bool warnOnZeroResults, ref IEnumerable<Pawn> __result)
            {
                // If settings disable this faction from using surrogates (or all surrogates are banned entirely), then there is no work to do here. Allow default generation to proceed.
                if (!ATReforged_Settings.surrogatesAllowed || !ATReforged_Settings.otherFactionsAllowedSurrogates || parms.faction == null || !Utils.FactionCanUseSkyMind(parms.faction.def))
                {
                    return;
                }

                try
                {
                    int nbHumanoids = 0;

                    foreach (Pawn pawn in __result)
                    {
                        // Count all non-trader pawns with humanlike intelligence that aren't drones.
                        if (pawn.def.race != null && pawn.def.race.Humanlike && pawn.trader == null && pawn.TraderKind == null && !Utils.IsConsideredMechanicalDrone(pawn))
                        {
                            nbHumanoids++;
                        }
                    }

                    // Skip groups that are too small
                    if (nbHumanoids <= ATReforged_Settings.minGroupSizeForSurrogates)
                    {
                        return;
                    }


                    List<Pawn> unalteredGroup = new List<Pawn>();
                    List<Pawn> ret = new List<Pawn>();
                    ISet<Pawn> candidatesForReplacement = __result.ToHashSet();

                    // Determine which pawns are ineligible and save them for the final group
                    foreach (Pawn pawn in candidatesForReplacement)
                    { 
                        if (pawn.def.race == null || !pawn.def.race.Humanlike || pawn.trader != null || pawn.TraderKind != null || pawn.kindDef.combatPower < 90f)
                        {
                            unalteredGroup.Add(pawn);
                        }
                    }

                    // Remove ineligible pawns from the candidates list
                    for (int i = 0; i < unalteredGroup.Count; i++)
                    { 
                        Pawn ineligiblePawn = unalteredGroup[i];
                        candidatesForReplacement.Remove(ineligiblePawn);
                    }

                    // Determine how many surrogates are taking the place of candidates
                    int surCount = (int)(candidatesForReplacement.Count * Rand.Range(ATReforged_Settings.minSurrogatePercentagePerLegalGroup, ATReforged_Settings.maxSurrogatePercentagePerLegalGroup));
                    if (surCount <= 0)
                    { // If rounding put the calculation below the minimum, set to the minimum value.
                        if (ATReforged_Settings.minSurrogatePercentagePerLegalGroup == 0.0f)
                            return;
                        else
                            surCount = 1;
                    }

                    // Remove random candidates until we have proper space for the surrogates - the removed pawns will not be chosen to be surrogates.
                    while (candidatesForReplacement.Count > surCount)
                    {
                        Pawn p = candidatesForReplacement.RandomElement();
                        unalteredGroup.Add(p);
                        candidatesForReplacement.Remove(p);
                    }

                    List<Pawn> candidateList = candidatesForReplacement.ToList();

                    // Generate all surrogate pawns individually and copy everything from the candidate
                    foreach (Pawn chosenCandidate in candidateList)
                    {
                        // Generate a possible surrogate that is less than or equal to the candidate in combat power and closest to it (or a random surrogate if none exist)
                        // This check is to ensure we don't spawn an insanely high power surrogate in the place of a really weak pawn.
                        PawnKindDef closestCombatPowerSurrogate = null;
                        foreach (PawnKindDef surrogateCandidate in Utils.ValidSurrogatePawnKindDefs)
                        {
                            if (closestCombatPowerSurrogate == null && surrogateCandidate.combatPower <= chosenCandidate.kindDef.combatPower)
                            {
                                closestCombatPowerSurrogate = surrogateCandidate;
                            }
                            else if (closestCombatPowerSurrogate != null && closestCombatPowerSurrogate.combatPower < surrogateCandidate.combatPower && surrogateCandidate.combatPower <= chosenCandidate.kindDef.combatPower)
                            { // The != null check is necessary here to avoid checking the .combatPower of a null PawnKindDef if it was null but couldn't assign above.
                                closestCombatPowerSurrogate = surrogateCandidate;
                            }
                        }

                        // If no valid surrogates were found to have a combat power low enough, pick a completely random surrogate.
                        if (closestCombatPowerSurrogate == null)
                            if (!Utils.ValidSurrogatePawnKindDefs.Any()) // If there are no valid surrogates at all, then panic and use the candidate
                            {
                                Log.Error("There are no valid surrogate pawn kind defs! No surrogates will ever spawn.");
                                ret.Add(chosenCandidate);
                            }
                            else
                            {
                                Log.Message("[ATR] The pawn's combat power of " + chosenCandidate.kindDef.combatPower + " was too low to find a suitable surrogate. A random one was chosen.");
                                closestCombatPowerSurrogate = Utils.ValidSurrogatePawnKindDefs.RandomElement();
                            }

                        // Generate the new surrogate according to the above calcuations with the correct faction.
                        Pawn surrogate = Utils.GenerateSurrogate(closestCombatPowerSurrogate, chosenCandidate.gender);

                        // If resulting surrogate is not a pawn that is not a valid surrogate type, abandon and use the original candidate pawn.
                        if (!Utils.ValidSurrogatePawnKindDefs.Contains(surrogate.kindDef))
                        {
                            Log.Warning("[ATR] Attempted to generate a surrogate of an illegal PawnKindDef. Aborting surrogate replacement attempt and using candidate pawn instead.");
                            surrogate.Destroy();
                            ret.Add(chosenCandidate);
                            continue;
                        }

                        // Connect the chosenCandidate to the surrogate as the controller. It is an external controller.
                        chosenCandidate.GetComp<CompSkyMindLink>().ConnectSurrogate(surrogate, true);

                        // The newly generated surrogate will take on the gear of its candidate - it must start with nothing first.
                        if (surrogate.inventory != null && surrogate.inventory.innerContainer != null)
                            surrogate.inventory.innerContainer.Clear();

                        surrogate.apparel.DestroyAll();

                        surrogate.equipment.DestroyAllEquipment();

                        // Give all equipment from the original candidate to the surrogate
                        if (chosenCandidate.equipment != null && surrogate.equipment != null)
                        {
                            foreach (ThingWithComps equipment in chosenCandidate.equipment.AllEquipmentListForReading.ToList())
                            {
                                try
                                {
                                    chosenCandidate.equipment.Remove(equipment);
                                    surrogate.equipment.AddEquipment(equipment);
                                }
                                catch (Exception er)
                                {
                                    Log.Warning("[ATR] Failed to transfer equipment to new surrogate in PawnGroupMakerUtility " + er.Message + " " + er.StackTrace);
                                }
                            }
                        }

                        // Transfer all apparel from the original candidate to the surrogate
                        if (chosenCandidate.apparel != null)
                        {
                            try
                            {
                                foreach (Apparel apparel in chosenCandidate.apparel.WornApparel.ToList())
                                {

                                    // Check to see if this apparel can be worn by the surrogate
                                    string path = "";
                                    if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                                    {
                                        path = apparel.def.apparel.wornGraphicPath;
                                    }
                                    else
                                    {
                                        path = $"{apparel.def.apparel.wornGraphicPath}_{surrogate.story.bodyType.defName}_south";
                                    }

                                    Texture2D appTex = null;
                                    // Seek apparel texture in the mods
                                    foreach (ModContentPack modPack in LoadedModManager.RunningModsListForReading)
                                    {
                                        appTex = modPack.GetContentHolder<Texture2D>().Get(path);
                                        if(appTex != null)
                                        {
                                            break;
                                        }
                                    }
                                    // Seek apparel texture in Rimworld itself
                                    if (appTex == null)
                                    {
                                        path = GenFilePaths.ContentPath<Texture2D>() + path;
                                        appTex = (Texture2D)(object)Resources.Load<Texture2D>(path);
                                    }

                                    // Only wear the apparel if a texture was found
                                    if (appTex != null)
                                    {
                                        chosenCandidate.apparel.Remove(apparel);
                                        surrogate.apparel.Wear(apparel);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning("[ATR] Failed to transfer a piece of apparel to a newly created surrogate. " + ex.Message + " " + ex.StackTrace);
                            }
                        }


                        // Transfer all inventory from the original candidate to the surrogate
                        if (chosenCandidate.inventory != null && chosenCandidate.inventory.innerContainer != null && surrogate.inventory != null && surrogate.inventory.innerContainer != null)
                        {
                            try
                            {
                                chosenCandidate.inventory.innerContainer.TryTransferAllToContainer(surrogate.inventory.innerContainer);

                                var elementsToRemove = new List<Thing>();
                                // Surrogates don't carry drugs, so delete any that the candidate happened to be holding that transfered
                                IList<Thing> surrogateInventory = surrogate.inventory.innerContainer;
                                foreach (Thing item in surrogateInventory)
                                {
                                    if (item.def.IsDrug)
                                        elementsToRemove.Add(item);
                                }
                                foreach (var elementToRemove in elementsToRemove)
                                    surrogate.inventory.innerContainer.Remove(elementToRemove);
                            }
                            catch (Exception ex)
                            {
                                Log.Warning("[ATR] Failed to transfer items to a newly created surrogate. " + ex.Message + " " + ex.StackTrace);
                            }
                        }
                        ret.Add(surrogate);
                    }
                    // All members of the group that were unaltered are returned to the final grouping.
                    __result = unalteredGroup.Concat(ret);

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
                }
                catch (Exception ex)
                {
                    Log.Warning("[ATR] Failed to generate surrogates when creating a Pawn Group. Originally spawned group is used instead." + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}