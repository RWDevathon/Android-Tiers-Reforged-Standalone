using RimWorld;

namespace ATReforged
{
    [DefOf]
        public static class TraitDefOf
        {
            static TraitDefOf()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
            }

            public static TraitDef FeelingsTowardOrganics;

            public static TraitDef Transhumanist;
        }
}
