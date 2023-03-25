using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_ThoughtDefOf
    {
        static ATR_ThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_ThoughtDefOf));
        }

        public static ThoughtDef ATR_ConnectedSkyMindAttacked;

        public static ThoughtDef ATR_AttackedViaSkyMind;

        public static ThoughtDef ATR_TrolledViaSkyMind;

        public static ThoughtDef ATR_SurrogateMentalBreak;


        public static ThoughtDef ATR_PersonalityShiftAllowed;

        public static ThoughtDef ATR_PersonalityShiftDenied;
    }
}
