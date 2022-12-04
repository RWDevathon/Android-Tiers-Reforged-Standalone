using Verse;
using HarmonyLib;
using System.Linq;
using RimWorld.Planet;
using RimWorld;

namespace ATReforged
{
    internal class JobGiver_GetHemogen_Patch
    {
        // Mechanical units are invalid targets for blood feeding.
        [HarmonyPatch(typeof(JobGiver_GetHemogen), "CanFeedOnPrisoner")]
        public class CanFeedOnPrisoner_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Pawn prisoner)
            {
                __result = __result && !Utils.IsConsideredMechanical(prisoner);
            }
        }
    }
}