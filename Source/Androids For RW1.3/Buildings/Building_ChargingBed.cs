using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    public class Building_ChargingBed : Building_Bed
    {

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

        }

        // If forcing a pawn to recharge is illegal for the given pawn, return why that is the case. If they can charge, return null.
        private FloatMenuOption CheckIfNotAllowed(Pawn pawn)
        { 
            // Check if the pawn can reach the building safely.
            if (!pawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            { 
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            }

            // Check if the building itself has power.
            if (!this.TryGetComp<CompPowerTrader>().PowerOn)
            { 
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            }

            // Check if the pawn is allowed to use its battery by settings.
            if (!Utils.CanUseBattery(pawn))
            { 
                return new FloatMenuOption("ATR_NeedToAllowCharge".Translate(), null);
            }

            // Check if the building has all of its unowned interaction spots used or if the pawn owns a slot in this bed.
            if(Medical || (!AnyUnownedSleepingSlot && pawn.ownership.OwnedBed != this))
            {
                return new FloatMenuOption("ATR_NoAvailableChargingSpots".Translate(), null);
            }

            // All checks passed, this pawn may be forced to charge. Return null.
            return null;
        }

        // Display the menu option for forcing to use the charging bed if it is legal.
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        { 
            base.GetFloatMenuOptions(myPawn);
            FloatMenuOption failureReason = CheckIfNotAllowed(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
            }
            // Yield an option to force the pawn to charge from the charging bed.
            else
            { 
                yield return new FloatMenuOption("ATR_ForceCharge".Translate(), delegate () {
                    IntVec3 chargingSpot;
                    // Locate a legal place for the pawn to claim.
                    for (int spotIndex = 0; spotIndex < TotalSleepingSlots; spotIndex++)
                    {
                        chargingSpot = GetSleepingSlotPos(spotIndex);
                        // If this particular spot is unoccupied and no one has reserved it, then it is open and can be claimed.
                        if (GetCurOccupantAt(chargingSpot) == null && !myPawn.Map.pawnDestinationReservationManager.IsReserved(chargingSpot))
                        {
                            myPawn.ownership.ClaimBedIfNonMedical(this);
                            Job job = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(this));
                            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            return;
                        }
                    }
                    // If this is reached, then something went wrong. The pawn will not claim the bed and will not start charging. Send a log message.
                    Log.Warning("[ATR] Pawn " + myPawn.Name + " was unable to claim a charging bed that was available! The order failed, and the pawn will not go to charge now.");
                });
            }
        }

    }
}
