using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using RimWorld.BaseGen;

namespace ATReforged
{
    internal class WorkGiver_Tend_Patch
    {
        // Patch the medical tend WorkGiver to not give doctoring jobs on mechanicals. WorkGiver_MechTend handles mechanical tending.
        [HarmonyPatch(typeof(WorkGiver_Tend), "HasJobOnThing")]
        public class PotentialWorkThingsGlobal_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn pawn, Thing t, bool forced, ref bool __result)
            {
                __result = !Utils.IsConsideredMechanical(t.def);
                return __result;
            }
        }
    }
}