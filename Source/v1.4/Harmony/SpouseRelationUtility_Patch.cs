using HarmonyLib;
using RimWorld;
using Verse;

namespace ATReforged
{
    public static class SpouseRelationUtility_Patch
    {
        // Mechanical units do not change their names when getting married.
        [HarmonyPatch(typeof(SpouseRelationUtility), "Roll_NameChangeOnMarriage")]
        public class Roll_NameChangeOnMarriage_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref MarriageNameChange __result)
            {
                if (Utils.IsConsideredMechanical(pawn))
                {
                    __result = MarriageNameChange.NoChange;
                    return false;
                }
                return true;
            }
        }
    }
}