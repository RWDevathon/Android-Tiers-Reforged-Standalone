using RimWorld;
using Verse;

namespace ATReforged
{
    public class ThoughtWorker_FeelingsTowardOrganics : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            TraitDef feel = DefDatabase<TraitDef>.GetNamed("FeelingsTowardOrganics", false);
            if(feel == null)
                return false;

            int feelingDegree = p.story.traits.DegreeOfTrait(feel);
            if (!RelationsUtility.PawnsKnowEachOther(p, other) || Utils.IsConsideredMechanical(other) || other.health.hediffSet.CountAddedAndImplantedParts() >= 5)
            {
                return false;
            }
            else
            {
                return ThoughtState.ActiveAtStage(feelingDegree - 1);
            }
        }
    }
}
