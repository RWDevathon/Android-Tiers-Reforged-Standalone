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
    internal class ThoughtWorker_Dark_Patch

    {
        // Mechanical units aren't bothered by darkness.
        [HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                //Already disabled => no more processing required
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanical(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}