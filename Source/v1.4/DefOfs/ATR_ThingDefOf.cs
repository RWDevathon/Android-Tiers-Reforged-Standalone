using Verse;
using RimWorld;


namespace ATReforged
{
    [DefOf]
    public static class ATR_ThingDefOf
    {
        static ATR_ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_ThingDefOf));
        }

        public static ThingDef ATR_BedsideChargerFacility;

        public static ThingDef HospitalBed;

        public static ThingDef ATR_FractalPill;

        public static ThingDef ATR_MaintenanceSpot;

        public static ThingDef ATR_MindOperationAttachedMote;
    }

}