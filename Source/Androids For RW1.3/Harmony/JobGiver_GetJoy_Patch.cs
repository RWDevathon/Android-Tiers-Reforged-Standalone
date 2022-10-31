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
    internal class JobGiver_GetJoy_Patch

    {
        // Mechanical Drones do not have joy needs.
        [HarmonyPatch(typeof(JobGiver_GetJoy), "TryGiveJob")]
        public class GetPriority_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Job __result)
            {
                if (Utils.IsConsideredMechanicalDrone(pawn))
                {
                    __result = null;
                }
            }
        }
    }
}