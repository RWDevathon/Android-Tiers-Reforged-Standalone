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
    internal class ThoughtWorker_ApparelDamaged_Patch
    {
        // Mechanical drones do not care about the state of their apparel.
        [HarmonyPatch(typeof(ThoughtWorker_ApparelDamaged), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                //Already disabled => no more processing required
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