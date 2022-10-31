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
    internal class ThoughtWorker_NeedJoy_Patch

    {
        
        // Mechanical drones don't have a joy need.
        [HarmonyPatch(typeof(ThoughtWorker_NeedJoy), "CurrentStateInternal")]
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