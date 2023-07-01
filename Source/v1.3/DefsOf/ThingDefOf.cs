using Verse;
using RimWorld;


namespace ATReforged
{
    [DefOf]
    public static class ThingDefOf
    {
        static ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }

        public static ThingDef MechFallBeam;

        public static ThingDef ATR_FractalPill;
    }

}