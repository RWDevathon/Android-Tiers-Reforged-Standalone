using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Mechanical units should do maintenance if they can't find anything else to do - this effectively replaces default vanilla's wandering behavior.
    public class JobGiver_DoMaintenanceIdle : ThinkNode_JobGiver
    {
        // Pawn ThinkTrees occasionally sort jobs to take on a priority. This is exceedingly low priority for idle maintenance.
        public override float GetPriority(Pawn pawn)
        {
            return 0.5f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompMaintenanceNeed compMaintenanceNeed = pawn.GetComp<CompMaintenanceNeed>();

            if (compMaintenanceNeed == null || pawn.InAggroMentalState)
            {
                return null;
            }

            // Idle maintenance is only not done if the maintenance level is too high to start the job effectively.
            if (compMaintenanceNeed.MaintenanceLevel > 0.95f)
            {
                return null;
            }

            // If this pawn's current position is legal for meditation, use it.
            if (ReservationUtility.CanReserve(pawn, pawn.Position) && MeditationUtility.SafeEnvironmentalConditions(pawn, pawn.Position, pawn.Map))
            {
                return JobMaker.MakeJob(ATR_JobDefOf.ATR_DoMaintenanceIdle, pawn.Position, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(pawn.Position));
            }

            MeditationSpotAndFocus meditationSpot = MeditationUtility.FindMeditationSpot(pawn);
            if (meditationSpot.IsValid)
            {
                return JobMaker.MakeJob(ATR_JobDefOf.ATR_DoMaintenanceIdle, meditationSpot.spot, new LocalTargetInfo(meditationSpot.spot.Cell));
            }
            return null;
        }
    }
}
