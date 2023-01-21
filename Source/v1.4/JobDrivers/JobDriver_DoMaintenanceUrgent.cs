using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    public class JobDriver_DoMaintenanceUrgent : JobDriver
    {
        protected const TargetIndex SpotInd = TargetIndex.A;

        protected const TargetIndex BedInd = TargetIndex.B;

        private const int TicksBetweenMotesBase = 100;

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
                    meditate = Toils_LayDownPower.LayDown(BedInd, FromBed, lookForOtherJobs: false, canSleep: false);
                }
                else
                {
                    meditate = Toils_LayDown.LayDown(BedInd, FromBed, lookForOtherJobs: false, canSleep: false);
                }
            }
            else
            {
                yield return Toils_Goto.GotoCell(SpotInd, PathEndMode.OnCell);
            }
            meditate.defaultCompleteMode = ToilCompleteMode.Delay;
            meditate.defaultDuration = JobEndInterval;
            meditate.FailOn(() => !MeditationUtility.SafeEnvironmentalConditions(pawn, TargetLocA, Map));
            meditate.FailOn(() => pawn.GetComp<CompMaintenanceNeed>().MaintenanceLevel >= pawn.GetComp<CompMaintenanceNeed>().TargetMaintenanceLevel);
            meditate.AddPreTickAction(delegate
            {
                MaintenanceTick();
            });
            yield return meditate;
        }

        protected void MaintenanceTick()
        {
            pawn.skills?.Learn(SkillDefOf.Intellectual, 0.0060000011f);
            pawn.GainComfortFromCellIfPossible();
            if (pawn.IsHashIntervalTick(TicksBetweenMotesBase))
            {
                FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Meditating);
            }
            pawn.GetComp<CompMaintenanceNeed>().ChangeMaintenanceLevel(0.00003f * ATReforged_Settings.maintenanceGainRateFactor);
        }
    }
}