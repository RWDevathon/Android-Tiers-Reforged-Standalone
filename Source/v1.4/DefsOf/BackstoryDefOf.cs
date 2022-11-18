using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class BackstoryDefOf
    {
        static BackstoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BackstoryDefOf));
        }

        public static BackstoryDef FreshBlank;
        public static BackstoryDef AdultBlank;

        public static BackstoryDef ATR_MechChildhood;
        public static BackstoryDef ATR_MechAdulthood;

        public static BackstoryDef ATR_DroneChildhood;
        public static BackstoryDef ATR_DroneAdulthood;

        public static BackstoryDef ATR_NewbootChildhood;
    }
}