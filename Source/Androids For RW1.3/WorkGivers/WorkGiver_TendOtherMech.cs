using Verse;

namespace ATReforged
{
    public class WorkGiver_TendOtherMech : WorkGiver_MechTend
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (base.HasJobOnThing(pawn, t, forced))
            {
                return pawn != t;
            }

            return false;
        }
    }
}
