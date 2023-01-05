using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Mechanical units should attempt maintenance if in poor maintenance and allowed to gain enough to escape it before doing other work.
    public class JobGiver_DoMaintenanceUrgent : ThinkNode_JobGiver
    {
        // Pawn ThinkTrees occasionally sort jobs to take on a priority. This is a relatively high priority job, and should almost always be done ahead of work.
        public override float GetPriority(Pawn pawn)
        {
            TimeAssignmentDef timeAssignmentDef = (pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment;
            if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
            {
                if (pawn.GetComp<CompMaintenanceNeed>()?.MaintenanceLevel < 0.1)
                {
                    return 8.5f;
                }
                return 5f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Work)
            {
                if (pawn.GetComp<CompMaintenanceNeed>()?.MaintenanceLevel < 0.1)
                {
                    return 8.5f;
                }
                return 6f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
            {
                if (pawn.GetComp<CompMaintenanceNeed>()?.MaintenanceLevel < 0.1)
                {
                    return 8.5f;
                }
                return 4f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
            {
                if (pawn.GetComp<CompMaintenanceNeed>()?.MaintenanceLevel < 0.1)
                {
                    return 8.5f;
                }
                return 4f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Meditate)
            {
                return 10f;
            }
            return 0.5f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompMaintenanceNeed compMaintenanceNeed = pawn.GetComp<CompMaintenanceNeed>();

            if (compMaintenanceNeed == null || pawn.InAggroMentalState)
            {
                return null;
            }

            // Urgent maintenance is done only if in poor or critical maintenance. It will never happen if the target level is below or equal to the poor maintenance threshold.
            if (compMaintenanceNeed.Stage > CompMaintenanceNeed.MaintenanceStage.Poor || compMaintenanceNeed.TargetMaintenanceLevel <= 0.3f)
            {
                return null;
            }

            // If conditions for keeping the job aren't satisfied, it would immediately end upon taking the job. Terminate before giving the job in this case.
            if (!MeditationUtility.CanMeditateNow(pawn) || !pawn.Spawned || !MeditationUtility.SafeEnvironmentalConditions(pawn, pawn.Position, pawn.Map))
            {
                return null;
            }

            return JobMaker.MakeJob(JobDefOf.ATR_DoMaintenanceUrgent, pawn.Position, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(pawn.Position));
        }
    }
}
