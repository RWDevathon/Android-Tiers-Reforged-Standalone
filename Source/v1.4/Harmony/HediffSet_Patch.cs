using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System;

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

                try
                {
                    // The targetHediffs cached are all hediffs which are temperature related and need to be checked against the hediffs present on this pawn.
                    HashSet<HediffDef> targetHediffDefs = Utils.GetTemperatureHediffDefsForRace(__instance.pawn.RaceProps);
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
                // If for whatever reason our detour fails, continue with vanilla behavior instead of erroring out.
                catch (Exception ex)
                {
                    return true;
                }
            }
        }
    }
}