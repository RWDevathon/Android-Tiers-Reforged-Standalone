using RimWorld;
using Verse;

namespace ATReforged
{
    [DefOf]
    public static class MentalStateDefOf
    {
        static MentalStateDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MentalStateDefOf));
        }
        public static MentalStateDef ATR_MentalState_Exterminator;
    }
}