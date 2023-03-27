using RimWorld;
using Verse;

namespace ATReforged
{
    public class ThoughtWorker_CoerciveVoiceSynthesizer : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            return RelationsUtility.PawnsKnowEachOther(p, other) && other.health.hediffSet.HasHediff(ATR_HediffDefOf.ATR_CoerciveVoiceSynthesizer);
        }
    }
}
