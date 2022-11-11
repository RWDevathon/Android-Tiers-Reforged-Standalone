using Verse;

namespace ATReforged
{
    public class HediffComp_Warper : HediffComp
    {
        public HediffCompProperties_Warper Props
        {
            get
            {
                return (HediffCompProperties_Warper)props;
            }
        }

        // Organics hit by a fractal lance suffer the fractal warping hediff. If they already have the hediff, then do nothing.
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostMake();
            if (Pawn.RaceProps.intelligence == Intelligence.Humanlike && !Utils.IsConsideredMechanical(Pawn) && Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FractalPillOrganic) == null)
            {
                Hediff fractal = HediffMaker.MakeHediff(HediffDefOf.FractalPillOrganic, Pawn);
                fractal.Severity = 0.25f;
                Pawn.health.AddHediff(fractal);
            }
        }
    }
}
