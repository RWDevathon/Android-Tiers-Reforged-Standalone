using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class IncidentDefOf
    {
        static IncidentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }
        public static IncidentDef ATR_HackingIncident;

        public static IncidentDef ResourcePodCrash;

        public static IncidentDef RefugeePodCrash;

        public static IncidentDef PsychicEmanatorShipPartCrash;

        public static IncidentDef DefoliatorShipPartCrash;
    }
}
