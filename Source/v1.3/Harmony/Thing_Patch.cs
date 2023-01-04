using Verse;
using HarmonyLib;

namespace ATReforged
{
    internal class Thing_Patch
    {
        // If Mechanical bio processors are not 1-1 efficient to humans (controlled by settings), then modify all ingested foods by the appropriate factor.
        [HarmonyPatch(typeof(Thing), "IngestedCalculateAmounts")]
        public class IngestedCalculateAmountsModifiedByBiogenEfficiency_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn ingester, ref float nutritionIngested)
            {
                // No patching is done on ingesting loss
                if (nutritionIngested <= 0f)
                    return;

                if (ATReforged_Settings.chargeCapableMeansDifferentBioEfficiency && Utils.CanUseBattery(ingester))
                {
                    nutritionIngested *= ATReforged_Settings.chargeCapableBioEfficiency;
                }
            }
        }
    }
}