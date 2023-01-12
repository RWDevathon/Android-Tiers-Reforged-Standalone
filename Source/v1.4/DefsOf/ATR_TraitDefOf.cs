using RimWorld;

namespace ATReforged
{
    [DefOf]
        public static class ATR_TraitDefOf
        {
            static ATR_TraitDefOf()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(ATR_TraitDefOf));
            }

            public static TraitDef ATR_FeelingsTowardOrganics;

            public static TraitDef Transhumanist;
        }
}
