using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_StatDefOf
    {
        static ATR_StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_StatDefOf));
        }

        public static StatDef ATR_MechanicalSurgerySuccessChance;

        public static StatDef ATR_MechanicalTendQuality;

        public static StatDef ATR_MechanicalTendSpeed;

        public static StatDef ATR_MechanicalTendQualityOffset;

        public static StatDef ATR_MechanicalSurgerySuccessChanceFactor;

        public static StatDef ATR_MaintenanceRetention;

        public static StatDef ATR_SurrogateLimitBonus;
    }
}