using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    internal class WorkGiver_PatientGoToBedTreatment_Patch
    {
        // Mechanical units need to check if there is a mechanic available, not if there is a doctor available, when seeking treatment.
        [HarmonyPatch(typeof(WorkGiver_PatientGoToBedTreatment), "AnyAvailableDoctorFor")]
        public class AnyAvailableDoctorFor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref bool __result)
            {
                // Only override the method for mechanical pawns.
                if (!Utils.IsConsideredMechanical(pawn))
                    return;

                // Don't worry about checks for map-less pawns. Vanilla behavior can handle that case.
                Map mapHeld = pawn.MapHeld;
                if (mapHeld == null)
                {
                    return;
                }

                // Attempt to locate an available mechanic in the faction.
                List<Pawn> list = mapHeld.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
                for (int i = 0; i < list.Count; i++)
                {
                    Pawn target = list[i];
                    if (target != pawn && (target.RaceProps.Humanlike || target.IsColonyMechPlayerControlled) && !target.Downed && target.Awake() && !target.InBed() && !target.InMentalState && !target.IsPrisoner && target.workSettings != null && target.workSettings.EverWork && target.workSettings.WorkIsActive(ATR_WorkTypeDefOf.ATR_Mechanic) && target.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && target.CanReach(pawn, PathEndMode.Touch, Danger.Deadly))
                    {
                        __result = true;
                        return;
                    }
                }

                // If no mechanic was found for a mechanical unit, even if there is a doctor available, set the result to false.
                __result = false;
            }
        }
    }
}