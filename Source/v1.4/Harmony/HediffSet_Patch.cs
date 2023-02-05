using Verse;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld;

namespace ATReforged
{
    public class HediffSet_Patch
    {
        // Mechanical units feel no pain.
        [HarmonyPatch(typeof(HediffSet), "CalculatePain")]
        public class CalculateMechanicalPain_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref HediffSet __instance, ref float __result)
            {
                if (Utils.IsConsideredMechanical(__instance.pawn))
                {
                    __result = 0f;
                    return false;
                }
                return true;
            }
        }

        // Mechanical units check against different hediffs than organics do for temperature hediff concerns.
        [HarmonyPatch(typeof(HediffSet), "HasTemperatureInjury")]
        public class HasTemperatureInjury_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(HediffSet __instance, ref bool __result, TemperatureInjuryStage minStage)
            {
                if (!Utils.IsConsideredMechanical(__instance.pawn))
                {
                    return true;
                }

                // Identify all hediffs that have a HediffGiver_Heat or HediffGiver_Hypothermia as their class (or is a sub-class of either) - these are temperature hediffs.
                if (!cachedTemperatureHediffs.ContainsKey(__instance.pawn.RaceProps))
                {
                    List<HediffGiverSetDef> hediffGiverSetDefs = __instance.pawn.RaceProps.hediffGiverSets;
                    List<HediffDef> targetHediffs = new List<HediffDef>();
                    foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                    {
                        foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                        {
                            if (typeof(HediffGiver_Heat).IsAssignableFrom(hediffGiver.GetType()) || typeof(HediffGiver_Hypothermia).IsAssignableFrom(hediffGiver.GetType()))
                            {
                                targetHediffs.Add(hediffGiver.hediff);
                            }
                        }
                    }
                    cachedTemperatureHediffs[__instance.pawn.RaceProps] = targetHediffs;
                }

                // The targetHediffs cached are all hediffs which are temperature related and need to be checked against the hediffs present on this pawn.
                List<HediffDef> targetHediffDefs = cachedTemperatureHediffs[__instance.pawn.RaceProps];
                if (targetHediffDefs.Count > 0)
                {
                    foreach (Hediff hediff in __instance.hediffs)
                    {
                        if (targetHediffDefs.Contains(hediff.def) && hediff.CurStageIndex > (int)minStage)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = false;
                return false;
            }
        }

        // Cached Hediffs for a particular pawn's race that count as temperature hediffs to avoid constant recalculation, cached when needed.
        static Dictionary<RaceProperties, List<HediffDef>> cachedTemperatureHediffs = new Dictionary<RaceProperties, List<HediffDef>> ();
    }
}