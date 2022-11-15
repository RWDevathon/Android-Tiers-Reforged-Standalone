﻿using Verse;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace ATReforged
{
    // Drones don't forget skills.
    [HarmonyPatch(typeof(SkillRecord), "Interval")]
    public static class Interval_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Pawn ___pawn)
        {
            if (Utils.IsConsideredMechanicalDrone(___pawn))
            {
                return false;
            }
            return true;
        }
    }
}