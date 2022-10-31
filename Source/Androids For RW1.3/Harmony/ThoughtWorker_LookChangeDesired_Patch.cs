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
    internal class ThoughtWorker_LookChangeDesired_Patch

    {
        // Mechanical drones don't care about their appearance.
        [HarmonyPatch(typeof(ThoughtWorker_LookChangeDesired), "CurrentStateInternal")]
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