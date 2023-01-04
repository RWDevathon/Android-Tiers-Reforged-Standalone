using HarmonyLib;
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

                bool hasAnyPawnWithRestNeed = false;
                for (int i = __instance.PawnsListForReading.Count - 1; i >= 0; i--)
                {
                    if (!Utils.IsConsideredMechanical(__instance.PawnsListForReading[i]) && __instance.PawnsListForReading[i].needs.rest != null)
                    {
                        hasAnyPawnWithRestNeed = true;
                        break;
                    }
                }
                __result = hasAnyPawnWithRestNeed;
            }
        }
    }
}