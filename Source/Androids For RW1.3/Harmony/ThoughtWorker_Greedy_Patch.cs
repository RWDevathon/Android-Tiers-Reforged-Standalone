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
    internal class ThoughtWorker_Greedy_Patch

    {
        // Mechanical drones aren't greedy. They shouldn't end up with this trait in the first place.
        [HarmonyPatch(typeof(ThoughtWorker_Greedy), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanicalDrone(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}