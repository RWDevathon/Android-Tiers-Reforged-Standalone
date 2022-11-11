using Verse;
using HarmonyLib;
using System.Linq;
using RimWorld.Planet;

namespace ATReforged
{
    internal class Caravan_Patch
    {
        // Prevent resting at night for a caravan if it does not have organics. Mechanical units don't rest.
        [HarmonyPatch(typeof(Caravan), "get_NightResting")]
        public class NightResting_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, ref Caravan __instance)
            {
                if (!__result)
                    return;
                __result = __instance.pawns.InnerListForReading.Any(pawn => !Utils.IsConsideredMechanical(pawn));
            }
        }
    }
}