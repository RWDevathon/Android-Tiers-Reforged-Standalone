using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace ATReforged
{
    internal class CompAbilityEffect_StartRitual_Patch
    {
        // Ideology-less units aren't involved with rituals.
        [HarmonyPatch(typeof(CompAbilityEffect_StartRitual), "get_Ritual")]
        public class CompAbilityEffect_StartRitual_get_Ritual_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Precept_Ritual __result, CompAbilityEffect_StartRitual __instance)
            {
                if (__instance.parent != null && __instance.parent.pawn != null && __instance.parent.pawn.ideo == null)
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }
    }
}