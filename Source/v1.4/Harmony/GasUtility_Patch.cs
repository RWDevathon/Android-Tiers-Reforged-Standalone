using Verse;
using HarmonyLib;

namespace ATReforged
{
    internal class GasUtility_Patch
    {
        // Mechanical units do not suffer gas exposure hediffs like Tox Gas.
        [HarmonyPatch(typeof(GasUtility), "ShouldGetGasExposureHediff")]
        public class ShouldGetGasExposureHediff_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Pawn pawn)
            {
                __result = __result && !Utils.IsConsideredMechanical(pawn);
            }
        }
    }
}