using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace ATReforged
{
    public static class Toils_LayDownPower
    {
        public const float GroundRestEffectiveness = 0.8f;

        private const int TicksBetweenFlecks = 200;

        private static readonly FleckDef fullChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_FullChargeFleck");
        private static readonly FleckDef halfChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_HalfChargeFleck");
        private static readonly FleckDef emptyChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_EmptyChargeFleck");

        public static Toil LayDown(TargetIndex chargingBuilding, bool hasBed)
        {
            Toil layDown = new Toil();

            // Responsible for handling information at the very start of the toil.
            layDown.initAction = delegate
            {
                Pawn actor = layDown.actor;
                actor.pather.StopDead();
                JobDriver curDriver = actor.jobs.curDriver;

                // Ensure that the pawn is in a bed if it was informed it was. Set the pawn posture and inform them of the bed they are now sleeping in.
                if (hasBed)
                {
                    Building_Bed bed = (Building_Bed)actor.CurJob.GetTarget(chargingBuilding).Thing;
                    if (!bed.OccupiedRect().Contains(actor.Position))
                    {
                        Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor);
                        actor.jobs.EndCurrentJob(JobCondition.Errored, true);
                        return;
                    }
                    actor.jobs.posture = PawnPosture.LayingInBed;
                    actor.mindState.lastBedDefSleptIn = bed.def;
                }
                else
                {
                    actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
                    actor.mindState.lastBedDefSleptIn = null;
                }

                // Initialize the sleeping tracking information, so that the pawn will receive mood buff/debuff if they sleep long enough.
                curDriver.asleep = true;
                if (actor.mindState.applyBedThoughtsTick == 0)
                {
                    actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
                    actor.mindState.applyBedThoughtsOnLeave = false;
                }

                // If the pawn does not own the bed, immediately remove positive thoughts.
                if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
                {
                    ThoughtUtility.RemovePositiveBedroomThoughts(actor);
                }

                // Set the device the actor is charging from to draw more power from the grid, according to the pawn's body size.
                actor.CurJob.GetTarget(chargingBuilding).Thing.TryGetComp<CompPowerTrader>().powerOutputInt -= Utils.GetPowerUsageByPawn(actor);

                // Mark the pawn as sleeping for targetting information for other factions.
                actor.GetComp<CompCanBeDormant>()?.ToSleep();
            };

            // Responsible for handling information done per tick of the action, including gaining energy and checking if the job fails for a particular reason.
            layDown.tickAction = delegate
            {
                Pawn actor = layDown.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                Building_Bed bed = null;
                Building_ChargingStation station = null;
                if (hasBed)
                {
                    bed = (Building_Bed)curJob.GetTarget(chargingBuilding).Thing;
                }
                else
                {
                    station = (Building_ChargingStation)curJob.GetTarget(chargingBuilding).Thing;
                }

                Need_Food foodNeed = actor.needs.food;
                Need_Rest restNeed = actor.needs.rest;

                // If the pawn is not asleep, check to see if they should sleep.
                if (!curDriver.asleep)
                {
                    // Pawn falls asleep if their rest need or food need is low enough.
                    if ((restNeed != null && restNeed.CurLevel < RestUtility.FallAsleepMaxLevel(actor)) || (foodNeed != null && foodNeed.CurLevelPercentage < 0.9f))
                    {
                        curDriver.asleep = true;
                    }
                }
                // if the pawn has their charge/rest need met (both if both exist, otherwise which ever one they have), then the job is complete.
                if (!HealthAIUtility.ShouldSeekMedicalRest(actor) &&!actor.Downed && (foodNeed == null || foodNeed.CurLevelPercentage >= 1.0f) && (bed == null || restNeed == null || restNeed.CurLevelPercentage >= 1.0f))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    return;
                }

                // Increment comfort from where the pawn is resting, if applicable.
                actor.GainComfortFromCellIfPossible();

                // If the pawn can gain rest, then gain rest.
                if (restNeed != null)
                {
                    float restEffectiveness = (bed == null || !bed.def.statBases.StatListContains(RimWorld.StatDefOf.BedRestEffectiveness)) ? RimWorld.StatDefOf.BedRestEffectiveness.valueIfMissing : bed.GetStatValue(RimWorld.StatDefOf.BedRestEffectiveness);
                    actor.needs.rest.TickResting(restEffectiveness);
                }

                // If the pawn is asleep and can charge (food not null), then charge.
                if (foodNeed != null)
                {
                    float chargeEffectiveness;
                    // Beds get charging effectiveness from their BedRestEffectiveness stat.
                    if (bed != null)
                    {
                        chargeEffectiveness = !bed.def.statBases.StatListContains(RimWorld.StatDefOf.BedRestEffectiveness) ? RimWorld.StatDefOf.BedRestEffectiveness.valueIfMissing : bed.GetStatValue(RimWorld.StatDefOf.BedRestEffectiveness);
                    }
                    else
                    {
                        chargeEffectiveness = !station.def.statBases.StatListContains(RimWorld.StatDefOf.BedRestEffectiveness) ? RimWorld.StatDefOf.BedRestEffectiveness.valueIfMissing : station.GetStatValue(RimWorld.StatDefOf.BedRestEffectiveness);
                    }
                    actor.needs.food.CurLevelPercentage += ATReforged_Settings.batteryPercentagePerRareTick / 250 * chargeEffectiveness;
                }

                // If the apply bed thought timer is up, set applyBedThoughtsOnLeave to true so that it will apply when done with the job.
                if (actor.mindState.applyBedThoughtsTick != 0 && actor.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
                {
                    actor.mindState.applyBedThoughtsTick += 60000;
                    actor.mindState.applyBedThoughtsOnLeave = true;
                }

                // Throw the appropriate flecks when appropriate. Charging flecks when charging (energy < rest), ZZZ's when resting (rest < energy), healing when injured and can naturally heal.
                if (actor.IsHashIntervalTick(TicksBetweenFlecks) && !actor.Position.Fogged(actor.Map))
                {
                    if (restNeed != null)
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.SleepZ);
                    }
                    if (foodNeed != null)
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, GetChargeFleckDef(foodNeed));
                    }
                    if (actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any())
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.HealingCross);
                    }
                }

                // If the pawn is downed and this is not a valid spot to stay in, then boot them to the closest walkable spot.
                if (actor.ownership != null && bed != null && !bed.Medical && !bed.OwnersForReading.Contains(actor))
                {
                    if (actor.Downed)
                    {
                        actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1, null);
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }

                // If the charging location is unusable for some reason, then abort the job. Downed pawns do not have a choice of exiting the job in this way.
                if (bed != null)
                {
                    if (!actor.Downed && (bed.Destroyed || bed.IsBrokenDown() || !(bool)bed.TryGetComp<CompPowerTrader>()?.PowerOn))
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        return;

                    }
                }
                else if (station != null)
                {
                    if (!actor.Downed && (station.Destroyed || station.IsBrokenDown() || !(bool)station.TryGetComp<CompPowerTrader>()?.PowerOn))
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        return;
                    }
                }
            };
            layDown.defaultCompleteMode = ToilCompleteMode.Never;
            if (hasBed)
            {
                layDown.FailOnBedNoLongerUsable(chargingBuilding);
            }

            // When the job is complete (failed or succeeded), handle the mood thought from sleeping and awaken.
            layDown.AddFinishAction(delegate
            {
                Pawn actor = layDown.actor;
                JobDriver curDriver = actor.jobs.curDriver;
                curDriver.asleep = false;

                // If the pawn was asleep long enough to have thoughts applied, apply them.
                if (actor.mindState.applyBedThoughtsOnLeave)
                {
                    ApplyBedThoughts(actor);
                }

                // Set the device the actor is charging from to stop drawing additional power from the grid, according to the pawn's body size.
                actor.CurJob.GetTarget(chargingBuilding).Thing.TryGetComp<CompPowerTrader>().powerOutputInt += Utils.GetPowerUsageByPawn(actor);
            });
            return layDown;
        }

        // Return a particular charge fleck depending on the charge of the food need tracker.
        private static FleckDef GetChargeFleckDef(Need_Food energyNeed)
        {
            if (energyNeed.CurLevelPercentage >= 0.80f) { return fullChargeFleck; }
            else if (energyNeed.CurLevelPercentage >= 0.40f) { return halfChargeFleck; }
            return emptyChargeFleck;
        }

        // Apply a sleep thought to pawns once they awaken (if they slept long enough to warrant one). This is functionally identical to Core's function in Toils_LayDown.ApplyBedThoughts.
        private static void ApplyBedThoughts(Pawn actor)
        {
            if (actor.needs.mood == null)
            {
                return;
            }

            MemoryThoughtHandler memThoughtHandler = actor.needs.mood.thoughts.memories;

            Building_Bed building_Bed = actor.CurrentBed();
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
            if (actor.GetRoom().PsychologicallyOutdoors)
            {
                memThoughtHandler.TryGainMemory(ThoughtDefOf.SleptOutside);
            }
            if (building_Bed == null || building_Bed.CostListAdjusted().Count == 0)
            {
                memThoughtHandler.TryGainMemory(ThoughtDefOf.SleptOnGround);
            }
            if (actor.AmbientTemperature < actor.def.GetStatValueAbstract(RimWorld.StatDefOf.ComfyTemperatureMin))
            {
                memThoughtHandler.TryGainMemory(ThoughtDefOf.SleptInCold);
            }
            if (actor.AmbientTemperature > actor.def.GetStatValueAbstract(RimWorld.StatDefOf.ComfyTemperatureMax))
            {
                memThoughtHandler.TryGainMemory(ThoughtDefOf.SleptInHeat);
            }
            if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners && !actor.story.traits.HasTrait(RimWorld.TraitDefOf.Ascetic))
            {
                ThoughtDef thoughtDef = null;
                if (building_Bed.GetRoom().Role == RoomRoleDefOf.Bedroom)
                {
                    thoughtDef = ThoughtDefOf.SleptInBedroom;
                }
                else if (building_Bed.GetRoom().Role == RoomRoleDefOf.Barracks)
                {
                    thoughtDef = ThoughtDefOf.SleptInBarracks;
                }
                if (thoughtDef != null)
                {
                    int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
                    if (thoughtDef.stages[scoreStageIndex] != null)
                    {
                        memThoughtHandler.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
                    }
                }
            }
            actor.Notify_AddBedThoughts();
        }
    }
}