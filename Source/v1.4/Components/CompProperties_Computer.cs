using Verse;

namespace ATReforged
{
    public class CompProperties_Computer : CompProperties
    {
        public CompProperties_Computer()
        {
            compClass = typeof(CompComputer);
        }

        public ServerType serverMode;
        public int passivePointGeneration = 0;
        public int percentageWorkBoost = 0;
        public int pointStorage = 0;
    }
}
