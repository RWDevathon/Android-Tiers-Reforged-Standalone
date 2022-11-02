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
    internal class ThoughtWorker_Precept_Patch
    {
        // Mechanical drones don't have precepts.
        [HarmonyPatch(typeof(ThoughtWorker_Precept), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {

            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanicalDrone(p) || Utils.IsSurrogate(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}