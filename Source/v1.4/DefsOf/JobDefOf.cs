using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class JobDefOf
    {
        static JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
        }
        public static JobDef RechargeBattery;

        public static JobDef ResurrectMechanical;

        public static JobDef TendMechanical;

        public static JobDef ATR_GenerateInsight;

        public static JobDef ATR_DoMaintenanceUrgent;

        public static JobDef ATR_DoMaintenanceIdle;
    }
}