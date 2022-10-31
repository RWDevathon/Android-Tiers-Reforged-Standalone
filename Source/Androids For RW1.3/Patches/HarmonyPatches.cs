using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace ATReforged
{
    [HarmonyPatch(typeof(JobDriver_Vomit))]
    [HarmonyPatch("MakeNewToils")]
    internal static class DeclineVomitJob
    {
        public static bool Prefix(ref JobDriver_Vomit __instance, ref IEnumerable<Toil> __result)
        {
            Pawn pawn = __instance.pawn;
            bool result;
            if (Utils.IsConsideredMechanical(pawn))
            {
                JobDriver_Vomit instance = __instance;
                __result = new List<Toil>
                {
                    new Toil
                    {
                        initAction = delegate()
                        {
                            instance.pawn.jobs.StopAll(false);
                        }
                    }
                };
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(HediffSet))]
    [HarmonyPatch("CalculatePain")]
    internal static class EstimatePainGivenOverride
    {
        public static bool Prefix(ref HediffSet __instance, ref float __result)
        {
            bool result;
            if (Utils.IsConsideredMechanical(__instance.pawn))
            {
                __result = 0f;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(CompFoodPoisonable))]
    [HarmonyPatch("PostIngested")]
    internal static class AndroidsFoodPoisonOverride
    {
        private static bool Prefix(CompFoodPoisonable __instance, Pawn ingester)
        {
            return Utils.IsConsideredMechanical(ingester);
        }
    }

}
