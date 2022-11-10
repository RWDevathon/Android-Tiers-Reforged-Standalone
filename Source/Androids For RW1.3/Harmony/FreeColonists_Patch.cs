using Verse;
using HarmonyLib;

namespace ATReforged
{
    internal class Thing_Patch
    {
        // If Mechanical bio processors are not 1-1 efficient to humans (controlled by settings), then modify all ingested foods by the appropriate factor.
        [HarmonyPatch(typeof(Thing), "Ingested")]
        public class Ingested_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn ingester, float nutritionWanted, ref float __result)
            {
                if (ATReforged_Settings.mechanicalsHaveDifferentBioprocessingEfficiency && (Utils.IsConsideredMechanicalAndroid(ingester) || Utils.IsConsideredMechanicalDrone(ingester)))
                {
                    __result *= ATReforged_Settings.mechanicalBioprocessingEfficiency;
                }
            }
        }
    }
}