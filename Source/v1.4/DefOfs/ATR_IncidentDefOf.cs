using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_IncidentDefOf
    {
        static ATR_IncidentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_IncidentDefOf));
        }
        public static IncidentDef ATR_HackingIncident;

        public static IncidentDef ResourcePodCrash;

        public static IncidentDef RefugeePodCrash;

        public static IncidentDef PsychicEmanatorShipPartCrash;

        public static IncidentDef DefoliatorShipPartCrash;
    }
}
