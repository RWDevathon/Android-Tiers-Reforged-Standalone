using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;
using RimWorld;

namespace ATReforged
{
    public class JobDriver_RechargeBattery : JobDriver
    {
        public Building_Bed Bed => job.GetTarget(TargetIndex.A).Thing as Building_Bed;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Bed != null && !pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed))
            {
                return false;
            }
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (TargetThingA is Building_Bed)
            {
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
                yield return Toils_Bed.GotoBed(TargetIndex.A);
                yield return Toils_LayDownPower.LayDown(TargetIndex.A, true);
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
                yield return Toils_LayDownPower.LayDown(TargetIndex.B, false);
            }
        }
    }
}