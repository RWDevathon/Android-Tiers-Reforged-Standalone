using Verse;
using RimWorld;

namespace ATReforged
{
    public class CompAndroidPod : ThingComp
    {
        public CompProperties_AndroidPod Props
        {
            get
            {
                return (CompProperties_AndroidPod)props;
            }

        }

        // Check contained pawns for power consumption and charging every 1 Tick.
        public override void CompTick()
        {
            HandlePowerExchange(0);
        }

        // Check contained pawns for power consumption and charging every 250 Ticks.
        public override void CompTickRare()
        { 
            HandlePowerExchange(1);
        }

        // Check contained pawns for power consumption and charging every 2000 Ticks.
        public override void CompTickLong()
        {
            HandlePowerExchange(2);
        }

        public void HandlePowerExchange(int tickerType)
        {
            if (!parent.TryGetComp<CompPowerTrader>().PowerOn)
            {
                return;
            }

            float powerConsumed = parent.TryGetComp<CompPowerTrader>().Props.basePowerConsumption;
            float powerExchanged = ATReforged_Settings.batteryPercentagePerRareTick * Props.chargingRate;

            // Depending on the tickerType used for the building, charge the appropriate amount for the unit. Baseline is 250 tick.
            switch (tickerType)
            {
                case 0: // CompTick - 1 Tick
                    powerExchanged /= 250;
                    break;
                case 1: // CompTickRare - 250 Tick [Default, no change, included for consistency]
                    break;
                case 2: // CompTickLong - 2000 Tick
                    powerExchanged *= 8;
                    break;
            }

            foreach (Pawn pawn in ((Building_Bed)parent).CurOccupants)
            {
                if (Utils.CanUseBattery(pawn) && pawn.needs.food != null)
                {
                    pawn.needs.food.CurLevelPercentage += powerExchanged;
                    powerConsumed += Utils.GetPowerUsageByPawn(pawn);
                    // Throwing a fleck every tick is not desirable. Only throw the mote if the comp is using rare or long.
                    if (tickerType != 0)
                        Utils.ThrowChargingFleck(pawn);
                }
            }
        }
    }
}