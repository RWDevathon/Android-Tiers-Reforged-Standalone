using RimWorld;
using Verse;

namespace ATReforged
{
    [DefOf]
    public static class BodyPartDefOf
    {
        static BodyPartDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartDefOf));
        }

        public static BodyPartDef ATR_InternalCorePump;

        public static BodyPartDef ATR_MechaniteStorage;
    }
}