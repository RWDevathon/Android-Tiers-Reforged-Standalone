using RimWorld;
using Verse.AI;
using Verse;

namespace ATReforged
{
    // Create an alternate version of the Tend WorkGiver so that mechanicals are only targetted by mechanics, and the WorkGiver will give the mechanic jobs instead of doctor jobs.
    public class WorkGiver_MechTend : WorkGiver_Tend
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn target) || !Utils.IsConsideredMechanical(t.def) || pawn.WorkTypeIsDisabled(ATR_WorkTypeDefOf.ATR_Mechanic) || (def.tendToHumanlikesOnly && !target.RaceProps.Humanlike) || (def.tendToAnimalsOnly && !target.RaceProps.Animal) || !GoodLayingStatusForTend(target, pawn) || !HealthAIUtility.ShouldBeTendedNowByPlayer(target) || !pawn.CanReserve(target, 1, -1, null, forced) || (target.InAggroMentalState && !target.health.hediffSet.HasHediff(HediffDefOf.Scaria)))
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn target = t as Pawn;
            Thing thing = HealthAIUtility.FindBestMedicine(pawn, target);
            if (thing != null)
            {
                return JobMaker.MakeJob(ATR_JobDefOf.ATR_TendMechanical, target, thing);
            }

            return JobMaker.MakeJob(ATR_JobDefOf.ATR_TendMechanical, target);
        }
    }
}
