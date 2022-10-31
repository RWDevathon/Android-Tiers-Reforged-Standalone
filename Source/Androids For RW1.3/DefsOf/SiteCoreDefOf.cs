using System;
using RimWorld;


namespace ATReforged
{
    [DefOf]
    public static class SitePartDefOf
    {
        static SitePartDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SitePartDefOf));
        }
        public static SitePartDef DownedT5Android;
    }
}