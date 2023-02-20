using System.Collections.Generic;
using Verse;

namespace ATReforged
{
    // This HediffComp will destroy any instances of a given HediffDef when the Hediff it is attached to is applied.
    public class HediffComp_RemoveHediffOnApply : HediffComp
    {
        public HediffCompProperties_RemoveHediffOnApply Props => (HediffCompProperties_RemoveHediffOnApply)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            HediffDef toRemove = Props.hediffToRemove;
            List<Hediff> hediffs = Pawn.health.hediffSet.hediffs;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (hediffs[i].def == toRemove)
                {
                    Pawn.health.RemoveHediff(hediffs[i]);
                }
            }
        }
    }
}
