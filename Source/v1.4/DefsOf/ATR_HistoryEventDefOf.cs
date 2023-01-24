using RimWorld;
using Verse;

namespace ATReforged
{
    [DefOf]
    public static class ATR_HistoryEventDefOf
    {
        static ATR_HistoryEventDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_HistoryEventDefOf));
        }

        public static HistoryEventDef ATR_PossessesOrganicColonist;

        public static HistoryEventDef ATR_PossessesMechanicalColonist;
    }
}