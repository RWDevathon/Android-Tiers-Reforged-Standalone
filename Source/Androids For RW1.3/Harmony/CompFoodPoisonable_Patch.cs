using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    // Mechanical units can not be food poisoned.
    internal class CompFoodPoisonable_Patch
    {
        [HarmonyPatch(typeof(CompFoodPoisonable), "PostIngested")]
        public class PostIngested_Patch
        {
            [HarmonyPrefix]
            private static bool Prefix(CompFoodPoisonable __instance, Pawn ingester)
            {
                return Utils.IsConsideredMechanical(ingester);
            }
        }
    }
}