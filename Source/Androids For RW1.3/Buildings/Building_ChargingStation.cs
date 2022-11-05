using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ATReforged
{
    public class Building_ChargingStation : Building
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
            // Check if the building itself has power and is not broken down.
            if (!this.TryGetComp<CompPowerTrader>().PowerOn || this.IsBrokenDown())
            { 
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            }
            // Check if the pawn is allowed to use its battery by settings.
            if (!Utils.CanUseBattery(pawn))
            { 
                return new FloatMenuOption("ATR_NeedToAllowCharge".Translate(), null);
            }

            // Check if the building has all of its interaction spots used.
            if (this.TryGetComp<CompChargingStation>().GetOpenRechargeSpot(pawn) == IntVec3.Invalid)
            {
                return new FloatMenuOption("ATR_NoAvailableChargingSpots".Translate(), null);
            }

            // Massive mechanical units may not use charging stations.
            if (Utils.IsConsideredMassive(pawn))
            {
                return new FloatMenuOption("ATR_MassiveNotAllowed".Translate(), null);
            }

            // All checks passed, this pawn may be forced to charge. Return null.
            return null;
        }

        // Display the menu option for forcing to use the charging station if it is legal.
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            base.GetFloatMenuOptions(pawn);
            FloatMenuOption failureReason = CheckIfNotAllowed(pawn);
            if (failureReason != null)
            {
                yield return failureReason;
            }
            else
            {
                yield return new FloatMenuOption("ATR_ForceCharge".Translate(), delegate () {
                    Job job = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(this.TryGetComp<CompChargingStation>().GetOpenRechargeSpot(pawn)), new LocalTargetInfo(this));
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
        }

        // If multiple pawns are selected, correctly identify pawns that can be told to charge and allow those pawns to do so as a group.
        public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            base.GetMultiSelectFloatMenuOptions(selPawns);
            List<Pawn> pawnsCanReach = new List<Pawn>();
            FloatMenuOption failureReason = null;

            // Generate a list of pawns that can use the station.
            foreach(Pawn pawn in selPawns)
            {
                failureReason = CheckIfNotAllowed(pawn);
                if (failureReason == null)
                {
                    pawnsCanReach.Add(pawn);
                }
            }

            // If there are no pawns that can reach, give a reason why. Note: It will only display the last failure reason detected.
            if (pawnsCanReach.NullOrEmpty())
            { 
                if (failureReason != null)
                    yield return failureReason;
                else
                    yield break;
            }
            else
            {
                 yield return new FloatMenuOption("ATR_ForceCharge".Translate(), delegate(){
                     CompChargingStation stationComp = this.TryGetComp<CompChargingStation>();

                     // Attempt to assign all pawns that can reach to the station a spot. If a pawn takes the last slot, then abort the process. Left-over pawns won't charge.
                     foreach (Pawn pawn in pawnsCanReach) {
                         IntVec3 reloadPlacePos = stationComp.GetOpenRechargeSpot(pawn);

                         if (reloadPlacePos == IntVec3.Invalid)
                             break;

                         Job job = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(reloadPlacePos), new LocalTargetInfo(this));
                         pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                     }
                 });
            }
        }

    }
}
