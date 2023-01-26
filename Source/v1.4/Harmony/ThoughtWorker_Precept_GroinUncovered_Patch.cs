using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class ThoughtWorker_Precept_GroinUncovered_Patch
    {
        // Mechanical units don't care about uncovered groins.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_GroinUncovered), "HasUncoveredGroin")]
        public class TW_Precept_GroinUncovered_HasUncoveredGroin
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref bool __result)
            {
                if (!__result)
                    return;

                if (Utils.IsConsideredMechanicalAndroid(p))
                {
                    __result = false;
                }
            }
        }
    }
}