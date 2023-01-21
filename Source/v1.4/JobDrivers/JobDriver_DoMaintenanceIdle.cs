using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Simplified version of DoMaintenanceUrgent that will continue to do this job until it is interrupted by something else or its timer elapses. Maintenance effects are applied when the job terminates.
    // It will not track how much maintenance it has, or its target maintenance level, allowing it to potentially waste time (but otherwise the pawn would just wander uselessly so this is fine).
    public class JobDriver_DoMaintenanceIdle : JobDriver
    {
        protected const TargetIndex SpotInd = TargetIndex.A;

        protected const TargetIndex BedInd = TargetIndex.B;

        private const int JobEndInterval = 4000;

        private bool FromBed => job.GetTarget(BedInd).Thing is Building_Bed;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(SpotInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil meditate = ToilMaker.MakeToil("MakeNewToils");
            meditate.socialMode = RandomSocialMode.Off;
            if (FromBed)
            {
                this.KeepLyingDown(BedInd);
                if (Utils.CanUseBattery(pawn))
                {
                    meditate = Toils_LayDownPower.LayDown(BedInd, FromBed, lookForOtherJobs: true, canSleep: false);
                }
                else
                {
                    meditate = Toils_LayDown.LayDown(BedInd, FromBed, lookForOtherJobs: true, canSleep: false);
                }
            }
            else
            {
                yield return Toils_Goto.GotoCell(SpotInd, PathEndMode.OnCell);
            }
            meditate.defaultCompleteMode = ToilCompleteMode.Delay;
            meditate.defaultDuration = JobEndInterval;
            meditate.FailOn(() => !MeditationUtility.SafeEnvironmentalConditions(pawn, TargetLocA, Map));
            meditate.AddFinishAction(delegate
            {
                pawn.GetComp<CompMaintenanceNeed>().ChangeMaintenanceLevel((Find.TickManager.TicksGame - startTick) * 0.00003f * ATReforged_Settings.maintenanceGainRateFactor);
            });
            yield return meditate;
        }
    }
}