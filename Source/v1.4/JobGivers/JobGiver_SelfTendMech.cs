using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    public class JobGiver_SelfTendMech : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike || !pawn.health.HasHediffsNeedingTend() || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.InAggroMentalState)
            {
                return null;
            }

            if (pawn.IsColonist && pawn.WorkTypeIsDisabled(ATR_WorkTypeDefOf.ATR_Mechanic))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(ATR_JobDefOf.ATR_TendMechanical, pawn);
            job.endAfterTendedOnce = true;
            return job;
        }
    }
}
