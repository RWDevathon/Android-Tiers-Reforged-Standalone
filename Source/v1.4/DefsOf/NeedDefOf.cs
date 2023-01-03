using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class NeedDefOf
    {
        static NeedDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(NeedDefOf));
        }
        // public static NeedDef ATR_MaintenanceNeed;
    }
}