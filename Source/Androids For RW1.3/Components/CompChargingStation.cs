using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;
using Verse.AI;

namespace ATReforged
{
    public class CompChargingStation : ThingComp
    {
        public CompProperties_ChargingStation Props
        {
            get
            {
                return (CompProperties_ChargingStation)props;
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            Utils.GCATPP.PopChargingStation((Building)parent);
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "ScheduledOff" || signal == "Breakdown" || signal == "PowerTurnedOff")
            {
                Utils.GCATPP.PopChargingStation((Building)parent);
            }

            if (signal == "PowerTurnedOn")
            {
                Utils.GCATPP.PushChargingStation((Building)parent);
            }
        }

        // Check adjacent pawns for power consumption and charging each Tick.
        public override void CompTick()
        {
            HandlePowerExchange(0);
        }

        // Check adjacent pawns for power consumption and charging every 250 Ticks.
        public override void CompTickRare()
        {
            HandlePowerExchange(1);
        }

        // Check adjacent pawns for power consumption and charging every 2000 Ticks.
        public override void CompTickLong()
        {
            HandlePowerExchange(2);
        }

        // Handle the power consumption and charging of this building and adjacent pawns. Charge pawns that have the appropriate job, and set the power consumption appropriately.
        public void HandlePowerExchange(int tickerType)
        {
            float powerConsumed = parent.TryGetComp<CompPowerTrader>().Props.basePowerConsumption;
            float powerExchanged = ATReforged_Settings.batteryPercentagePerRareTick;

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

            // Check all adjacent tiles for pawns that can (and are asking for) charging. Charge them and add their power consumption.
            foreach (IntVec3 adjPos in ((Building)parent).CellsAdjacent8WayAndInside().ToList())
            {
                foreach (Thing thing in adjPos.GetThingList(parent.Map).ToList())
                {
                    if (thing is Pawn pawn && Utils.CanUseBattery(pawn) && pawn.CurJobDef == JobDefOf.ATPP_GoReloadBattery)
                    {
                        pawn.needs.food.CurLevelPercentage += powerExchanged;
                        powerConsumed += Utils.GetPowerUsageByPawn(pawn);
                        // Throwing a mote every tick is not desirable. Only throw the mote if the comp is using rare or long.
                        if (tickerType != 0)
                            Utils.ThrowChargingFleck(pawn);
                    }
                }
            }
            parent.TryGetComp<CompPowerTrader>().powerOutputInt = -powerConsumed;
        }

        // Return the first available spot on this station. Return IntVec3.Invalid if there is none.
        public IntVec3 GetOpenRechargeSpot(Pawn pawn)
        {
            foreach (IntVec3 adjPos in ((Building)parent).CellsAdjacent8WayAndInside())
            {
                if (pawn.CanReach(new LocalTargetInfo(adjPos), PathEndMode.OnCell, Danger.Deadly) && !pawn.Map.pawnDestinationReservationManager.IsReserved(adjPos))
                    return adjPos;
            }
            return IntVec3.Invalid;
        }
    }
}