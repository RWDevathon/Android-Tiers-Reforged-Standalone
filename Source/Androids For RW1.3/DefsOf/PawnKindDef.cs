using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class PawnKindDefOf
    {
        static PawnKindDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
        }

        public static PawnKindDef T5Colonist;

        public static PawnKindDef AbominationAtlas;

        public static PawnKindDef M7MechPawn;

        public static PawnKindDef M8MechPawn;

        public static PawnKindDef MicroScyther;
    }
}