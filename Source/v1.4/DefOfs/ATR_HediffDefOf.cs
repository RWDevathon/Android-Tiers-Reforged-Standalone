using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_HediffDefOf
    {
        static ATR_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_HediffDefOf));
        }
        public static HediffDef ATR_FractalPillOrganic;

        public static HediffDef ATR_StasisPill;

        public static HediffDef ATR_RemainingCharge;

        public static HediffDef ATR_RecoveringFromDDOS;

        public static HediffDef ATR_ShortReboot;

        public static HediffDef ATR_LongReboot;

        public static HediffDef ATR_MemoryCorruption;

        public static HediffDef ATR_AutomodulatedVoiceSynthesizer;

        public static HediffDef ATR_CoerciveVoiceSynthesizer;

        // SkyMind related Hediffs

        public static HediffDef ATR_SplitConsciousness;

        public static HediffDef ATR_ForeignConsciousness;

        public static HediffDef ATR_MindOperation;

        public static HediffDef ATR_IsolatedCore;

        public static HediffDef ATR_AutonomousCore;

        public static HediffDef ATR_ReceiverCore;

        public static HediffDef ATR_SkyMindReceiver;

        public static HediffDef ATR_SkyMindTransceiver;

        public static HediffDef ATR_NoController;

        // Maintenance Part failures

        public static HediffDef ATR_PartDecay;

        public static HediffDef ATR_RustedPart;

        public static HediffDef ATR_PowerLoss;

        public static HediffDef ATR_DamagedCore;

        public static HediffDef ATR_FailingCoolantValves;

        public static HediffDef ATR_RogueMechanites;

        // Maintenance effects

        public static HediffDef ATR_MaintenanceCritical;

        public static HediffDef ATR_MaintenancePoor;

        public static HediffDef ATR_MaintenanceSatisfactory;

    }
}
