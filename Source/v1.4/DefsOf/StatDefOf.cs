using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class StatDefOf
    {
        static StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
        }

        public static StatDef MechanicalSurgerySuccessChance;

        public static StatDef MechanicalOperationSpeed;

        public static StatDef MechanicalTendQuality;

        public static StatDef MechanicalTendSpeed;

        public static StatDef MechanicalTendQualityOffset;

        public static StatDef MechanicalSurgerySuccessChanceFactor;

        public static StatDef ATR_MaintenanceRetention;
    }
}