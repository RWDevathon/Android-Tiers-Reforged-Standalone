using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ATReforged
{
    class IngestionOutcomeDoer_GiveTwoHediffs : IngestionOutcomeDoer
    {
        public HediffDef hediffDef_Organic = new HediffDef();
        public HediffDef hediffDef_Mechanical = new HediffDef();
        public float severity = -1f;
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (!Utils.IsConsideredMechanical(pawn))
            {
                Hediff hediff = HediffMaker.MakeHediff(hediffDef_Organic, pawn);
                float num;
                if (severity > 0f)
                {
                    num = severity;
                }
                else
                {
                    num = hediffDef_Organic.initialSeverity;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }
            else
            {
                Hediff hediff = HediffMaker.MakeHediff(hediffDef_Mechanical, pawn);
                float num;
                if (severity > 0f)
                {
                    num = severity;
                }
                else
                {
                    num = hediffDef_Mechanical.initialSeverity;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (parentDef.IsDrug && chance >= 1f)
            {
                foreach (StatDrawEntry s in hediffDef_Organic.SpecialDisplayStats(StatRequest.ForEmpty()))
                {
                    yield return s;
                }
            }
            yield break;
        }
    }
}


