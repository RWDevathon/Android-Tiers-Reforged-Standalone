using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    // Drones and surrogates don't forget skills.
    [HarmonyPatch(typeof(SkillRecord), "Interval")]
    public static class Interval_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Pawn ___pawn)
        {
            if (Utils.IsConsideredMechanicalDrone(___pawn) || Utils.IsSurrogate(___pawn))
            {
                return false;
            }
            return true;
        }
    }
}