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
    internal class ThoughtWorker_BondedAnimalMaster_Patch
    {
        // Mechanical drones do not care about not being the master of a bonded animal. They shouldn't bond to animals in the first place.
        [HarmonyPatch(typeof(ThoughtWorker_BondedAnimalMaster), "CurrentStateInternal")]
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