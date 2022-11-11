using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Alternate WorkGiver for TendSelfMech to use the appropriate Mechanical stats.
    public class WorkGiver_TendSelfEmergencyMech : WorkGiver_TendSelfMech
    {
        private static JobGiver_SelfTendMech jgp = new JobGiver_SelfTendMech();

        public override Job NonScanJob(Pawn pawn)
        {
            if (!HasJobOnThing(pawn, pawn) || !HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(pawn))
            {
                return null;
            }

            ThinkResult thinkResult = jgp.TryIssueJobPackage(pawn, default);
            if (thinkResult.IsValid)
            {
                return thinkResult.Job;
            }

            return null;
        }
    }
}
