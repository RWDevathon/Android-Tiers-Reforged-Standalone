using Verse;

namespace ATReforged
{
    public class CompProperties_SuperComputer : CompProperties
    {
        public CompProperties_SuperComputer()
        {
            compClass = typeof(CompSuperComputer);
        }

        public int passivePointGeneration = 0;
        public int percentageWorkBoost = 0;
        public int pointStorage = 0;
    }
}
