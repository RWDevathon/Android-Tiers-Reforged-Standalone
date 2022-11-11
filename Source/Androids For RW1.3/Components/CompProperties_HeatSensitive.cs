using Verse;


namespace ATReforged
{
    public class CompProperties_HeatSensitive : CompProperties
    {
        public CompProperties_HeatSensitive()
        {
            compClass = typeof(CompHeatSensitive);
        }

        public float safeHeat = 20;
        public float warningHeat = 30;
        public float dangerHeat = 35;
    }
}
