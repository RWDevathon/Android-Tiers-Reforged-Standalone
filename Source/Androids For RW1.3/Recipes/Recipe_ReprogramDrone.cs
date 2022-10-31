using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ATReforged
{
    public class Recipe_ReprogramDrone : Recipe_SurgeryAndroids
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                pawn.health.AddHediff(recipe.addsHediff, part, null);
                if (!CheckSurgeryFailAndroid(billDoer, pawn, ingredients, part, null))
                {
                    TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                    {
                        billDoer,
                        pawn
                    });
                    pawn.SetFaction(Faction.OfPlayer, null);
                    Find.LetterStack.ReceiveLetter("ATR_ReprogramSuccess".Translate(), "ATR_ReprogramSuccessDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.PositiveEvent, pawn, null);
                }
                else if (Rand.Chance(0.2f))
                {
                    Hediff corruption = HediffMaker.MakeHediff(HediffDefOf.ATR_MemoryCorruption, pawn, part);
                    corruption.Severity = Rand.Range(0.15f, 0.95f);
                    pawn.health.AddHediff(corruption, part, null);
                }
                Find.LetterStack.ReceiveLetter("ATR_ReprogramFailed".Translate(), "ATR_ReprogramFailedDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.NegativeEvent, pawn);
            }
        }
    }
}
