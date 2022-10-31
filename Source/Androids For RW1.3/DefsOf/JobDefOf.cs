using System;
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
        public static JobDef ATPP_GoReloadBattery;

        public static JobDef ResurrectMechanical;

        public static JobDef TendMechanical;
    }
}