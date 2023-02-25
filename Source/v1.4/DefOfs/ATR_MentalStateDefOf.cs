using RimWorld;
using Verse;

namespace ATReforged
{
    [DefOf]
    public static class ATR_MentalStateDefOf
    {
        static ATR_MentalStateDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_MentalStateDefOf));
        }
        public static MentalStateDef ATR_MentalState_Exterminator;
    }
}