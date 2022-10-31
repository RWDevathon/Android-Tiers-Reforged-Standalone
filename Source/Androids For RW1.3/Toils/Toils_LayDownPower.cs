using System;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace ATReforged
{
    public static class Toils_LayDownPower
    {
        public const float GroundRestEffectiveness = 0.8f;

        public static Toil LayDown(TargetIndex bedOrRestSpotIndex, bool hasBed, bool lookForOtherJobs, bool canSleep = true, bool gainRestAndHealth = true)
        {
            Toil layDown = new Toil();
            layDown.initAction = delegate
            {
                Pawn actor = layDown.actor;
                actor.pather.StopDead();
                JobDriver curDriver = actor.jobs.curDriver;
                if (hasBed)
                {
                    Building_Bed t = (Building_Bed)actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing;
                    if (!t.OccupiedRect().Contains(actor.Position))
                    {
                        Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor);
                        actor.jobs.EndCurrentJob(JobCondition.Errored, true);
                        return;
                    }
                    actor.jobs.posture = PawnPosture.LayingInBed;
                }
                else
                {
                    actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
                }
                curDriver.asleep = false;
                if (actor.mindState.applyBedThoughtsTick == 0)
                {
                    actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
                    actor.mindState.applyBedThoughtsOnLeave = false;
                }
                if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
                {
                    ThoughtUtility.RemovePositiveBedroomThoughts(actor);
                }
            };

            layDown.tickAction = delegate
            {
                Pawn actor = layDown.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                Building_Bed building_Bed = (Building_Bed)curJob.GetTarget(bedOrRestSpotIndex).Thing;
                actor.GainComfortFromCellIfPossible();

                if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged(actor.Map))
                {
                    if (curDriver.asleep)
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.SleepZ);
                    }
                    if (gainRestAndHealth && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.HealingCross);
                    }
                }
                if (actor.ownership != null && building_Bed != null && !building_Bed.Medical && !building_Bed.OwnersForReading.Contains(actor))
                {
                    if (actor.Downed)
                    {
                        actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1, null);
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }
                if (lookForOtherJobs && actor.IsHashIntervalTick(211))
                {
                    actor.jobs.CheckForJobOverride();
                    return;
                }

                if ( actor.needs.food.CurLevelPercentage >= 1.0f
                    || building_Bed.Destroyed || building_Bed.IsBrokenDown()
                     || !building_Bed.TryGetComp<CompPowerTrader>().PowerOn)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                }
            };
            layDown.defaultCompleteMode = ToilCompleteMode.Never;
            if (hasBed)
            {
                layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
            }
            layDown.AddFinishAction(delegate
            {
                Pawn actor = layDown.actor;
                JobDriver curDriver = actor.jobs.curDriver;
                curDriver.asleep = false;
            });
            return layDown;
        }
    }
}