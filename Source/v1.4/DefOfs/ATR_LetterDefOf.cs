using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_LetterDefOf
    {
        static ATR_LetterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_LetterDefOf));
        }

        public static LetterDef ATR_PersonalityShiftLetter;

        public static LetterDef ATR_PersonalityShiftRequestLetter;
    }
}
