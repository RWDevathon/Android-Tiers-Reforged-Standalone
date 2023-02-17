using Verse;

namespace ATReforged
{
    // This Hediff class appears only on surrogates of other factions, and exists solely to display a tethered mote above the pawn.
    public class Hediff_ForeignConsciousness : HediffWithComps
    {
        public override bool ShouldRemove => pawn.Downed;

        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();

            pawn.health.RemoveHediff(this);
        }
    }
}
