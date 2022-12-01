using Verse;
using HarmonyLib;
using RimWorld;

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

                // If charging efficiency differences are enabled, the unit can charge, and is a player pawn (to avoid issues with foreign pawns not bringing enough food), then modify it.
                if (ATReforged_Settings.chargeCapableMeansDifferentBioEfficiency && Utils.CanUseBattery(ingester) && ingester.Faction == Faction.OfPlayer)
                {
                    nutritionIngested *= ATReforged_Settings.chargeCapableBioEfficiency;
                }
            }
        }
    }
}