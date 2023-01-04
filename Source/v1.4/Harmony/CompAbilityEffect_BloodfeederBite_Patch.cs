using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class CompAbilityEffect_BloodfeederBite_Patch
    {
        // Mechanical units are invalid targets for blood feeding.
        [HarmonyPatch(typeof(CompAbilityEffect_BloodfeederBite), "Valid")]
        public class Valid_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, LocalTargetInfo target)
            {
                __result = __result && !Utils.IsConsideredMechanical(target.Pawn);
            }
        }
    }
}