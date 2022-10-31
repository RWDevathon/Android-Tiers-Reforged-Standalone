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
    internal class Thought_IdeoDisrespectedBuilding_Patch

    {
        // Mechanical drones do not have ideological qualms about buildings.
        [HarmonyPatch(typeof(Thought_IdeoDisrespectedBuilding), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn ___pawn, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanicalDrone(___pawn))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}