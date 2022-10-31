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
    internal class ThoughtWorker_Precept_Social_Patch

    {
        // Mechanical drones don't have precepts. Other pawns don't judge them for this.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_Social), "CurrentSocialStateInternal")]
        public class TW_Precept_Social_CurrentSocialStateInternal
        {

            [HarmonyPostfix]
            public static void Listener(Pawn p, Pawn otherPawn, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanicalDrone(p) || Utils.IsConsideredMechanicalDrone(p) || Utils.IsSurrogate(p) || Utils.IsSurrogate(otherPawn))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}