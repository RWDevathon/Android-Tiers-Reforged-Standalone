using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ATReforged
{
    public class Recipe_ReprogramDrone : Recipe_SurgeryAndroids
    {
        // This recipe is specifically targetting the brain of a mechanical unit, so we only need to check if the brain is available (a slight optimization over checking fixed body parts).
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {

            BodyPartRecord targetBodyPart = pawn.health.hediffSet.GetBrain();
            if (targetBodyPart != null && (Utils.IsConsideredMechanicalDrone(pawn) || Utils.IsConsideredMechanicalAnimal(pawn)))
            {
                yield return targetBodyPart;
            }
            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                pawn.health.AddHediff(recipe.addsHediff, part, null);
                // Handle success state
                if (!CheckSurgeryFailAndroid(billDoer, pawn, ingredients, part, null))
                {
                    TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                    {
                        billDoer,
                        pawn
                    });
                    pawn.SetFaction(Faction.OfPlayer, null);
                    Find.LetterStack.ReceiveLetter("ATR_ReprogramSuccess".Translate(), "ATR_ReprogramSuccessDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.PositiveEvent, pawn, null);
                    return;
                }
                // Handle fail state, with a 20% chance for especially bad effects occurring.
                if (Rand.Chance(0.2f))
                {
                    Hediff corruption = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_MemoryCorruption, pawn, part);
                    corruption.Severity = Rand.Range(0.15f, 0.95f);
                    pawn.health.AddHediff(corruption, part, null);
                }
                Find.LetterStack.ReceiveLetter("ATR_ReprogramFailed".Translate(), "ATR_ReprogramFailedDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.NegativeEvent, pawn);
            }
        }
    }
}