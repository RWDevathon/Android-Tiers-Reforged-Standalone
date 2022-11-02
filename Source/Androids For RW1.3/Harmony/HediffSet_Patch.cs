using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ATReforged
{
    [HarmonyPatch(typeof(HediffSet), "CalculatePain")]
    internal static class CalculateMechanicalPain_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref HediffSet __instance, ref float __result)
        {
            if (Utils.IsConsideredMechanical(__instance.pawn))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }
}