using RimWorld;
using Verse.AI;
using Verse;

namespace ATReforged
{
    // Target controller for using the Resurrection Kit
    public class CompTargetEffect_ResurrectMechanical : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            // Only player controlled pawns that can reach the target can use the kit.
            if (user.Faction == Faction.OfPlayer && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
            {
                Job job = JobMaker.MakeJob(ATR_JobDefOf.ATR_ResurrectMechanical, target, parent);
                job.count = 1;
                user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }
    }
}
