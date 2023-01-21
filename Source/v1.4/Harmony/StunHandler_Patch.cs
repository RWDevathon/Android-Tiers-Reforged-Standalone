using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class StunHandler_Patch
    {
        // Mechanical units are vulnerable to EMP.
        [HarmonyPatch(typeof(StunHandler), "get_AffectedByEMP")]
        public class get_AffectedByEMP_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Thing ___parent)
            {
                // No need to do any checks if it is already true.
                if (__result)
                    return;

                if (___parent is Pawn pawn && Utils.IsConsideredMechanical(pawn))
                {
                    __result = true;
                }
            }
        }
    }
}