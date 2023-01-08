using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class PrisonBreakUtility_Patch
    {
        // Drones can not participate in prison breaks.
        [HarmonyPatch(typeof(PrisonBreakUtility), "CanParticipateInPrisonBreak")]
        public class CanParticipateInPrisonBreak_Patch
        {
            [HarmonyPostfix]
            public static void Listener( ref bool __result, Pawn pawn)
            {
                __result = __result && !Utils.IsConsideredMechanicalDrone(pawn);
            }
        }
    }
}