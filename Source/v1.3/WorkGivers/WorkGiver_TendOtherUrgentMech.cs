using RimWorld;
using Verse;

namespace ATReforged
{
    public class WorkGiver_TendOtherUrgentMech : WorkGiver_MechTend
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (base.HasJobOnThing(pawn, t, forced))
            {
                return HealthAIUtility.ShouldBeTendedNowByPlayerUrgent((Pawn)t);
            }

            return false;
        }
    }
}
