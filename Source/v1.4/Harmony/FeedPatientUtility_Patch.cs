using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    // There is no reason to try and feed a mechanical unit that is charging.
    internal class FeedPatientUtility_Patch
    {
        [HarmonyPatch(typeof(FeedPatientUtility), "ShouldBeFed")]
        public class ShouldBeFed_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref bool __result)
            {
                if (__result && Utils.IsConsideredMechanical(p) && p.CurJob.def == JobDefOf.RechargeBattery)
                    __result = false;
            }
        }
    }
}