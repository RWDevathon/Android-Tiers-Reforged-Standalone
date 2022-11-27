using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    // Mechanical units can not be food poisoned, so set their chance to receive food poisoning to zero.
    internal class FoodUtility_Patch
    {
        [HarmonyPatch(typeof(FoodUtility), "GetFoodPoisonChanceFactor")]
        public class GetFoodPoisonChanceFactor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn ingester, ref float __result)
            {
                if (Utils.IsConsideredMechanical(ingester))
                    __result = 0f;
            }
        }
    }
}