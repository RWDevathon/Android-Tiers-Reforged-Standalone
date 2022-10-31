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

        public static ThingDef Tier1Android;

        public static ThingDef Tier2Android;

        public static ThingDef Tier3Android;

        public static ThingDef Tier4Android;

        public static ThingDef Tier5Android;

        public static ThingDef M7Mech;

        public static ThingDef M8Mech;

        public static ThingDef MechFallBeam;

        public static ThingDef HospitalBed;

        public static ThingDef FractalPill;
    }

}