using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ATReforged
{
    public class Building_ChargingBed : Building_Bed
    {
        // If forcing a pawn to recharge is illegal for the given pawn, return why that is the case. If they can charge, return null.
        private FloatMenuOption CheckIfNotAllowed(Pawn pawn)
        { 
            // Check if the pawn can reach the building safely.
            if (!pawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            { 
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            }

            // Check if the building itself has power.
            if (!GetComp<CompPowerTrader>().PowerOn)
            { 
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            }

            // Check if the pawn is allowed to use its battery by settings.
            if (!Utils.CanUseBattery(pawn) || pawn.needs.food == null)
            { 
                return new FloatMenuOption("ATR_NeedToAllowCharge".Translate(pawn), null);
            }

            // Check if the building has all of its unowned interaction spots used or if the pawn owns a slot in this bed.
            if (!AnyUnoccupiedSleepingSlot && pawn.ownership.OwnedBed != this)
            {
                return new FloatMenuOption("ATR_NoAvailableChargingSpots".Translate(), null);
            }

            // Check if the pawn is too big for the bed.
            if (def.building.bed_maxBodySize < pawn.BodySize)
            {
                return new FloatMenuOption("ATR_MassiveNotAllowed".Translate(), null);
            }

            // All checks passed, this pawn may be forced to charge. Return null.
            return null;
        }

        // Display the menu option for forcing to use the charging bed if it is legal.
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        { 
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(myPawn))
            {
                yield return option;
            }

            FloatMenuOption failureReason = CheckIfNotAllowed(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
            }
            // Yield an option to force the pawn to charge from the charging bed.
            else
            {
                yield return new FloatMenuOption("ATR_ForceCharge".Translate(), delegate ()
                {
                    myPawn.ownership.ClaimBedIfNonMedical(this);
                    Job job = new Job(ATR_JobDefOf.ATR_RechargeBattery, new LocalTargetInfo(this));
                    if (Medical)
                    {
                        job.restUntilHealed = true;
                    }
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
        }
    }
}
