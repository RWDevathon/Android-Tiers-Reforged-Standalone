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
                try
                {
                    if (__result && Utils.IsConsideredMechanical(___pawn)) 
                    {
                        __result = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("[ATR] ATReforged.JobDriver_Vomit_Patch Encountered an error while attempting to check if a pawn should vomit. The pawn will vomit." + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}