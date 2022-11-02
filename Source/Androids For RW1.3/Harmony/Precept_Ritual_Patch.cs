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
    internal class Precept_Ritual_Patch

    {
        // Mechanical drones do not have ideological obligations.
        [HarmonyPatch(typeof(Precept_Ritual), "AddObligation")]
        public class AddObligation_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(RitualObligation obligation)
            {
                try
                {
                    if (obligation.targetA.Thing != null
                        && ((obligation.targetA.Thing is Pawn pawn && Utils.IsConsideredMechanicalDrone(pawn))
                             || (obligation.targetA.Thing is Corpse corpse && Utils.IsConsideredMechanicalDrone(corpse.InnerPawn))))
                        return false;
                    else
                        return true;
                }
                catch (Exception e)
                {
                    Log.Message("[ATR] Precept_Ritual.AddObligation : " + e.Message + " - " + e.StackTrace);
                    return true;
                }
            }
        }

    }
}