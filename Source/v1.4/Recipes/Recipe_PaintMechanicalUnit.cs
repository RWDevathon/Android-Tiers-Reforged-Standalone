using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ATReforged
{
    public class Recipe_PaintMechanicalUnit : Recipe_SurgeryAndroids
    {
        // This recipe always targets the core part, and is always applicable to "alien" mechanical races, as it would throw errors if not using AlienRace.
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (!Utils.IsConsideredMechanical(pawn) || !(pawn.def is AlienRace.ThingDef_AlienRace))
            {
                yield break;
            }

            yield return pawn.RaceProps.body.corePart;
        }

        // On completion, open a dialog menu to select the new paint color.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Find.WindowStack.Add(new Dialog_Repaint(pawn));
        }
    }
}
