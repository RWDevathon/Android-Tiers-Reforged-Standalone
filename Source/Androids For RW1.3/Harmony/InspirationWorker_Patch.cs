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
    internal class InspirationWorker_Patch

    {
        // Drones and surrogates don't get inspirations.
        [HarmonyPatch(typeof(InspirationWorker), "InspirationCanOccur")]
        public class InspirationCanOccur_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref bool __result)
            {
                if (Utils.IsSurrogate(pawn) || Utils.IsConsideredMechanicalDrone(pawn))
                    __result = false;
            }
        }
    }
}