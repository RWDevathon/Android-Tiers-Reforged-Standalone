using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace ATReforged
{
    public class JobDriver_GoReloadBattery : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Downed) { return false; }
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (TargetThingA is Building_Bed pod)
            {
                yield return Toils_Bed.GotoBed(TargetIndex.A);
                yield return Toils_LayDownPower.LayDown(TargetIndex.A, true, false, false, true);
            }
            else
            {
                Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
                Toil nothing = new Toil();
                yield return gotoCell;
                Toil setSkin = new Toil
                {
                    initAction = delegate { pawn.Rotation = Rot4.South; }
                };
                yield return setSkin;
                yield return nothing;
                yield return Toils_General.Wait(250);
                yield return Toils_Jump.JumpIf(nothing, () => pawn.needs.food.CurLevelPercentage < 0.95f
                    && !job.targetB.ThingDestroyed && !((Building)job.targetB).IsBrokenDown()
                    && ((Building)job.targetB).TryGetComp<CompPowerTrader>().PowerOn);
            }
        }
    }
}