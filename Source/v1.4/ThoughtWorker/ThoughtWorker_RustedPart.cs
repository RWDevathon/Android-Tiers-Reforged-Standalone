using RimWorld;
using Verse;

namespace ATReforged
{
    public class ThoughtWorker_RustedPart : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (ThoughtUtility.ThoughtNullified(p, def) || !Utils.IsConsideredMechanicalAndroid(p))
            {
                return ThoughtState.Inactive;
            }

            for (int i = p.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                if (p.health.hediffSet.hediffs[i].def == ATR_HediffDefOf.ATR_RustedPart)
                    return true;
            }
            return ThoughtState.Inactive;
        }
    }
}
