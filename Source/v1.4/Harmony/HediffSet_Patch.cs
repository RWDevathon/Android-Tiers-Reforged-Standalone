using Verse;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld;

namespace ATReforged
{
    internal class HediffSet_Patch
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

                // Identify all hediffs that have a HediffGiver_Heat or HediffGiver_Hypothermia as their class - these are temperature hediffs.
                List<HediffGiverSetDef> hediffGiverSetDefs = __instance.pawn.RaceProps.hediffGiverSets;
                List<HediffDef> targetHediffs = new List<HediffDef>();
                foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                {
                    foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                    {
                        if (hediffGiver.GetType() == typeof(HediffGiver_Heat) || hediffGiver.GetType() == typeof(HediffGiver_Hypothermia))
                        {
                            targetHediffs.Add(hediffGiver.hediff);
                        }
                    }
                }

                // The targetHediffs taken in the last step are all hediffs which are temperature related and need to be checked against the hediffs present on this pawn.
                foreach (Hediff hediff in __instance.hediffs)
                {
                    if (targetHediffs.Contains(hediff.def) && hediff.CurStageIndex > (int)minStage)
                    {
                        __result = true;
                        return false;
                    }
                }
                __result = false;
                return false;
            }
        }
    }
}