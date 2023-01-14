using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_RulePackDefOf
    {
        static ATR_RulePackDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_RulePackDefOf));
        }

        public static RulePackDef ATR_AndroidNoneNames;

        public static RulePackDef ATR_DroneNoneNames;
    }
}
