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