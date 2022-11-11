using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class HediffDefOf
    {
        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
        public static HediffDef FractalPillOrganic;

        public static HediffDef FractalPillMechanical;

        public static HediffDef ATR_RemainingCharge;

        public static HediffDef RecoveringFromDDOS;

        public static HediffDef ATR_ShortReboot;

        public static HediffDef ATR_LongReboot;

        public static HediffDef ATR_MemoryCorruption;

        public static HediffDef ATR_IsolatedCore;

        public static HediffDef ATR_AutonomousCore;

        public static HediffDef ATR_ReceiverCore;

        public static HediffDef ATR_SkyMindReceiver;

        public static HediffDef ATR_SkyMindTransceiver;

        public static HediffDef ATR_MindOperation;

        public static HediffDef ATR_NoController;
    }
}
