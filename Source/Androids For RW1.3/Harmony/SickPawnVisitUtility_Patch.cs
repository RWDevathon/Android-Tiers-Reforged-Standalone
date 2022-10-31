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
    internal class SickPawnVisitUtility_Patch

    {
        // Drones do not receive medical visitors.
        [HarmonyPatch(typeof(SickPawnVisitUtility), "CanVisit")]
        public class CanVisit_Patch
        {
            [HarmonyPrefix]
            public static void Listener(Pawn pawn, Pawn sick, JoyCategory maxPatientJoy, ref bool __result)
            {
                if (Utils.IsConsideredMechanicalDrone(sick))
                {
                    __result = false;
                }
            }
        }
    }
}