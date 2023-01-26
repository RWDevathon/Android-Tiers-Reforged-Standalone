using System;
using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class JobDriver_Vomit_Patch
    {
        // Override job for getting food based on whether the pawn can charge instead or not.
        [HarmonyPatch(typeof(JobDriver_Vomit), "TryMakePreToilReservations")]
        public class TryMakePreToilReservations_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref Pawn ___pawn, ref bool __result)
            {
                __result = __result && !Utils.IsConsideredMechanical(___pawn);
            }
        }
    }
}