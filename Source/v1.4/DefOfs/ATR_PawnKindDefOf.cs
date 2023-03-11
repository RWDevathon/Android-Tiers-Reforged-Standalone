using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_PawnKindDefOf
    {
        static ATR_PawnKindDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_PawnKindDefOf));
        }

        public static PawnKindDef ATR_FractalAbomination;

        public static PawnKindDef ATR_MicroScyther;

        public static PawnKindDef ATR_FractalWitness;
    }
}