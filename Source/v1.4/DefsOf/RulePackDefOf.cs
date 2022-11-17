using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class RulePackDefOf
    {
        static RulePackDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RulePackDefOf));
        }

        public static RulePackDef ATR_AndroidMaleNames;

        public static RulePackDef ATR_AndroidFemaleNames;

        public static RulePackDef ATR_AndroidNoneNames;

        public static RulePackDef ATR_DroneNoneNames;
    }
}
