using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class ThoughtUtility_Patch
    {
        // Other pawns don't care about executed drones or surrogates.
        [HarmonyPatch(typeof(ThoughtUtility), "GiveThoughtsForPawnExecuted")]
        public class GiveThoughtsForPawnExecuted_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn victim, PawnExecutionKind kind)
            {
                if (Utils.IsConsideredMechanicalDrone(victim) || Utils.IsSurrogate(victim))
                    return false;
                else
                    return true;
            }
        }

        // Other pawns don't care about "organs" harvested from drones or surrogates.
        [HarmonyPatch(typeof(ThoughtUtility), "GiveThoughtsForPawnOrganHarvested")]
        public class GiveThoughtsForPawnOrganHarvested_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn victim)
            {
                if (Utils.IsConsideredMechanicalDrone(victim) || Utils.IsSurrogate(victim))
                    return false;
                else
                    return true;
            }
        }
    }
}