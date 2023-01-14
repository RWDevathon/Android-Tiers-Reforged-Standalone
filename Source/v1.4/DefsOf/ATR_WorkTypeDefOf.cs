using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_WorkTypeDefOf
    {
        static ATR_WorkTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_WorkTypeDefOf));
        }

        public static WorkTypeDef ATR_Mechanic;
    }
}