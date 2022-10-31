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
    internal class InteractionUtility_Patch
    {
        // Mechanical drones don't start social interactions.
        [HarmonyPatch(typeof(InteractionUtility), "CanInitiateInteraction")]
        public class CanInitiateInteraction_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref bool __result, InteractionDef interactionDef = null)
            {
                __result = __result && !Utils.IsConsideredMechanicalDrone(pawn);
            }
        }

        // Mechanical drones don't receive social interactions.
        [HarmonyPatch(typeof(InteractionUtility), "CanReceiveInteraction")]
        public class CanReceiveInteraction_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref bool __result, InteractionDef interactionDef = null)
            {
                __result = __result && !Utils.IsConsideredMechanicalDrone(pawn);
            }
        }
    }
}