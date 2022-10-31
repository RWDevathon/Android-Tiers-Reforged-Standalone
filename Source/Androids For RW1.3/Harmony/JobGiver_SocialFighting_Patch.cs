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
    internal class JobGiver_SocialFighting_Patch

    {
        // Social fighting does not occur with mechanical drones.
        [HarmonyPatch(typeof(JobGiver_SocialFighting), "TryGiveJob")]
        public class GetPriority_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Job __result)
            {
                Pawn otherPawn = ((MentalState_SocialFighting)pawn.MentalState).otherPawn;
                if (Utils.IsConsideredMechanicalDrone(pawn) || Utils.IsConsideredMechanicalDrone(otherPawn))
                {
                    __result = null;
                }
            }
        }
    }
}