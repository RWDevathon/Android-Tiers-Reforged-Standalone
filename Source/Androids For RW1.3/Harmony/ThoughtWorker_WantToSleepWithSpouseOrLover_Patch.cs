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
    internal class ThoughtWorker_WantToSleepWithSpouseOrLover_Patch
    {
        // Mechanical and Cloud pawns don't trigger spouse/lover sleeping needs.
        [HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                try
                {
                    if (!__result.Active)
                        return;

                    Pawn otherPawn = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false).otherPawn;

                    if (Utils.IsConsideredMechanical(p) || Utils.IsConsideredMechanical(otherPawn) || Utils.GCATPP.GetCloudPawns().Contains(p) || Utils.GCATPP.GetCloudPawns().Contains(otherPawn))
                        __result = false;
                }
                catch(Exception e)
                {
                    Log.Message("[ATTP] ThoughtWorker_WantToSleepWithSpouseOrLover.CurrentStateInternal " + e.Message + " " + e.StackTrace);
                }
            }
        }
    }
}