using Verse;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace ATReforged
{
    // It is necessary to patch the SickPawnVisitUtility and prefix its CanVisit function as it checks for the rest need without null-checking so it throws an error for pawns that don't need rest.
    [HarmonyPatch(typeof(SickPawnVisitUtility), "CanVisit")]
    public static class CanVisit_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Pawn sick, JoyCategory maxPatientJoy, ref bool __result)
        {
            if (pawn.needs?.rest != null)
            {
                return true;
            }

            // Run a shorter modified version of the default one to avoid the rest need.
            if (sick.IsColonist && !sick.IsSlave && !pawn.IsSlave && pawn.RaceProps.Humanlike && !sick.Dead && pawn != sick && sick.InBed() && !sick.IsForbidden(pawn) && sick.needs.joy != null && (int)sick.needs.joy.CurCategory <= (int)maxPatientJoy && InteractionUtility.CanReceiveInteraction(sick) && pawn.CanReserveAndReach(sick, PathEndMode.InteractionCell, Danger.None))
            {
                __result = !AboutToRecover(sick);
                return false;
            }
            __result = false;
            return false;
        }

        private static bool AboutToRecover(Pawn pawn)
        {
            if (pawn.Downed)
            {
                return false;
            }

            if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) && !HealthAIUtility.ShouldSeekMedicalRest(pawn))
            {
                return true;
            }

            if (pawn.health.hediffSet.HasImmunizableNotImmuneHediff())
            {
                return false;
            }

            return true;
        }
    }
}