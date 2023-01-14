using RimWorld;
using Verse;

namespace ATReforged
{
    [DefOf]
    public static class ATR_BodyPartDefOf
    {
        static ATR_BodyPartDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_BodyPartDefOf));
        }

        public static BodyPartDef ATR_InternalCorePump;

        public static BodyPartDef ATR_MechaniteStorage;
    }
}