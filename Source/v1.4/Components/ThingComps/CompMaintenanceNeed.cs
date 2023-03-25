using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ATReforged
{
    [StaticConstructorOnStartup]
    public class CompMaintenanceNeed : ThingComp
    {
        Pawn Pawn => (Pawn)parent;

        private static float ThresholdCritical => 0.1f;

        private static float ThresholdPoor => 0.3f;

        private static float ThresholdSatisfactory => 0.7f;

        public static readonly List<float> MaintenanceThresholdBandPercentages = new List<float> {0f, ThresholdCritical, ThresholdPoor, ThresholdSatisfactory, 1f};

        private static readonly float TicksPerDay = 60000;

        private static readonly float TicksPerLong = 2000;

        private static readonly SimpleCurve PartDecayContractChance = new SimpleCurve
        {
            new CurvePoint(3f, 9999999f),
            new CurvePoint(6f, 48f),
            new CurvePoint(12f, 24f),
            new CurvePoint(24f, 12f)
        };

        private static readonly SimpleCurve RustContractChance = new SimpleCurve
        {
            new CurvePoint(3f, 9999999f),
            new CurvePoint(6f, 48f),
            new CurvePoint(12f, 24f),
            new CurvePoint(24f, 12f)
        };

        private static readonly SimpleCurve PowerLossContractChance = new SimpleCurve
        {
            new CurvePoint(3f, 9999999f),
            new CurvePoint(6f, 60f),
            new CurvePoint(12f, 30f),
            new CurvePoint(24f, 15f)
        };

        private static readonly SimpleCurve CoreDamageContractChance = new SimpleCurve
        {
            new CurvePoint(5f, 9999999f),
            new CurvePoint(8f, 5000f),
            new CurvePoint(16f, 2400f),
            new CurvePoint(32f, 120f)
        };

        private static readonly SimpleCurve FailingValvesContractChance = new SimpleCurve
        {
            new CurvePoint(5f, 9999999f),
            new CurvePoint(8f, 2400f),
            new CurvePoint(16f, 160f),
            new CurvePoint(32f, 80f)
        };

        private static readonly SimpleCurve RogueMechanitesContractChance = new SimpleCurve
        {
            new CurvePoint(5f, 9999999f),
            new CurvePoint(8f, 6400f),
            new CurvePoint(16f, 3200f),
            new CurvePoint(32f, 240f)
        };

        public enum MaintenanceStage
        {
            Critical,
            Poor,
            Sufficient,
            Satisfactory
        }

        public MaintenanceStage Stage
        {
            get
            {
                if (maintenanceLevel < ThresholdCritical)
                    return MaintenanceStage.Critical;
                else if (maintenanceLevel < ThresholdPoor)
                    return MaintenanceStage.Poor;
                else if (maintenanceLevel < ThresholdSatisfactory)
                    return MaintenanceStage.Sufficient;
                return MaintenanceStage.Satisfactory;
            }
        }

        public float MaintenanceLevel
        {
            get
            {
                return maintenanceLevel;
            }
        }

        public float TargetMaintenanceLevel
        {
            get
            {
                return targetLevel;
            }
            set
            {
                targetLevel = Mathf.Clamp(value, 0f, 1f);
            }
        }

        private float DailyFallPerStage(MaintenanceStage stage)
        {
            switch (stage)
            {
                case MaintenanceStage.Critical:
                    return 0.03f; // 3% per day base (10 -> 0 should take 3.3 days with 1 efficiency)
                case MaintenanceStage.Poor:
                    return 0.06f; // 6% per day base (30 -> 10 should take 3.3 days with 1 efficiency)
                case MaintenanceStage.Sufficient:
                    return 0.12f; // 12% per day base (70 -> 30 should take 3.3 days with 1 efficiency)
                default:
                    return 0.24f; // 24% per day base (100 -> 70 should take 1.25 days with 1 efficiency)
            }
        }

        public float MaintenanceFallPerDay()
        {
            return Mathf.Clamp(DailyFallPerStage(Stage) * ATReforged_Settings.maintenanceFallRateFactor / Pawn.GetStatValue(ATR_StatDefOf.ATR_MaintenanceRetention, cacheStaleAfterTicks: 2000), 0.005f, 2f);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (maintenanceLevel < 0)
            {
                maintenanceLevel = 0.6f;
            }
            if (targetLevel < 0)
            {
                targetLevel = 0.5f;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref maintenanceLevel, "ATR_maintenanceLevel", -1);
            Scribe_Values.Look(ref targetLevel, "ATR_targetLevel", -1);
            Scribe_Values.Look(ref cachedFallRatePerDay, "ATR_cachedFallRatePerDay", -1);
            Scribe_Values.Look(ref maintenanceEffectTicks, "ATR_maintenanceEffectTicks", TicksPerDay);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!Pawn.Spawned || !ATReforged_Settings.maintenanceNeedExists || Find.TickManager.TicksGame % 2000 != 0)
            {
                return;
            }

            // Recache fall rate every in game day.
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                cachedFallRatePerDay = MaintenanceFallPerDay();
            }

            // If maintenance has been low for at least 3 days (modified by settings), issues can begin manifesting.
            if (maintenanceEffectTicks < (-180000 * ATReforged_Settings.maintenancePartFailureRateFactor))
            {
                TryPoorMaintenanceCheck();
            }

            ChangeMaintenanceEffectTicks();
            ChangeMaintenanceLevel(-cachedFallRatePerDay * TicksPerLong / TicksPerDay);
        }

        // Alter the maintenance level by the provided amount (decreases are assumed to be negative). Ensure the level never falls outside 0 - 1 range and handle stage changes appropriately.
        public void ChangeMaintenanceLevel(float baseChange)
        {
            MaintenanceStage currentStage = Stage;
            maintenanceLevel = Mathf.Clamp(maintenanceLevel + baseChange, 0f, 1f);

            // If we changed stages, make sure we initialize an appropriate stage effect hediff if there is one. They remove themselves automatically when appropriate.
            if (Stage != currentStage)
            {
                switch (Stage)
                {
                    case MaintenanceStage.Critical:
                        Hediff criticalHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_MaintenanceCritical, Pawn);
                        Pawn.health.AddHediff(criticalHediff);
                        break;
                    case MaintenanceStage.Poor:
                        Hediff poorHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_MaintenancePoor, Pawn);
                        Pawn.health.AddHediff(poorHediff);
                        break;
                    case MaintenanceStage.Satisfactory:
                        Hediff satisfactoryHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_MaintenanceSatisfactory, Pawn);
                        Pawn.health.AddHediff(satisfactoryHediff);
                        break;
                }
            }
        }

        /// <summary>
        /// Alter the maintenance effect ticks based on the provided tick rate. Actual effect is changed based on the pawn's maintenance level.
        /// If less than 0.3 (poor maintenance), effect trends negatively, and if higher than 0.7, effect trends positively.
        /// Between 0.3 and 0.7, effect trends toward -60000 or 60000 representing one full day of poor or high maintenance, and does not change if between -60000 and 60000 already.
        /// </summary>
        public void ChangeMaintenanceEffectTicks()
        {
            if (maintenanceLevel < 0.3f)
            {
                maintenanceEffectTicks -= TicksPerLong;

                if (maintenanceEffectTicks > 0)
                {
                    maintenanceEffectTicks -= maintenanceEffectTicks * 0.001f;
                }
            }
            else if (maintenanceLevel > 0.7f)
            {
                maintenanceEffectTicks += TicksPerLong;

                if (maintenanceEffectTicks < 0)
                {
                    maintenanceEffectTicks -= maintenanceEffectTicks * 0.001f;
                }
            }
            else
            {
                maintenanceEffectTicks = Mathf.MoveTowards(maintenanceEffectTicks, 0, 1f * TicksPerLong);
            }
            // Prevent the ticks from going outside a 60 day value positively or negatively.
            maintenanceEffectTicks = Mathf.Clamp(maintenanceEffectTicks, -3600000, 3600000);
        }

        public void SendPartFailureLetter(Pawn pawn, Hediff cause)
        {
            if (PawnUtility.ShouldSendNotificationAbout(pawn) && ATReforged_Settings.receiveMaintenanceFailureLetters)
            {
                Find.LetterStack.ReceiveLetter("LetterHealthComplicationsLabel".Translate(pawn.LabelShort, cause.LabelCap, pawn.Named("PAWN")).CapitalizeFirst(), "LetterHealthComplications".Translate(pawn.LabelShortCap, cause.LabelCap, "ATR_PoorMaintenanceLetter".Translate(), pawn.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn);
            }
        }

        // Maintenance need has associated gizmos for displaying and controlling the maintenance level of pawns.
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!ATReforged_Settings.maintenanceNeedExists)
            {
                yield break;
            }

            if (Find.Selector.SingleSelectedThing == parent)
            {
                Gizmo_MaintenanceStatus maintenanceStatusGizmo = new Gizmo_MaintenanceStatus
                {
                    maintenanceNeed = this
                };
                yield return maintenanceStatusGizmo;

                Gizmo_MaintenanceEffect maintenanceEffectGizmo = new Gizmo_MaintenanceEffect
                {
                    maintenanceNeed = this
                };
                yield return maintenanceEffectGizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action subtract20PercentMaintenance = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance -20%",
                    action = delegate
                    {
                        ChangeMaintenanceLevel(-0.2f);
                    }
                };
                yield return subtract20PercentMaintenance;
                Command_Action add20PercentMaintenance = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance +20%",
                    action = delegate
                    {
                        ChangeMaintenanceLevel(0.2f);
                    }
                };
                yield return add20PercentMaintenance;
                Command_Action substract1DayMaintenanceEffect = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance Effect -1 day",
                    action = delegate
                    {
                        maintenanceEffectTicks -= TicksPerDay;
                    }
                };
                yield return substract1DayMaintenanceEffect;
                Command_Action add1DayMaintenanceEffect = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance Effect +1 day",
                    action = delegate
                    {
                        maintenanceEffectTicks += TicksPerDay;
                    }
                };
                yield return add1DayMaintenanceEffect;
            }
            yield break;
        }

        public string MaintenanceTipString()
        {
            if (maintenanceLevelInfoCached == null)
            {
                for (int stageInt = 0; stageInt < MaintenanceThresholdBandPercentages.Count - 1; stageInt++)
                {
                    maintenanceLevelInfoCached += "ATR_MaintenanceLevelInfoRange".Translate((MaintenanceThresholdBandPercentages[stageInt] * 100f).ToStringDecimalIfSmall(), (MaintenanceThresholdBandPercentages[stageInt + 1] * 100f).ToStringDecimalIfSmall()) + ": " + "ATR_MaintenanceLevelInfoFallRate".Translate(DailyFallPerStage((MaintenanceStage)stageInt).ToStringPercent()) + "\n";
                }
            }
            return (("ATR_MaintenanceGizmoLabel".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + MaintenanceLevel.ToStringPercent("0.#") + "\n" + "ATR_MaintenanceTargetLabel".Translate() + ": " + TargetMaintenanceLevel.ToStringPercent("0.#") + "\n\n" + "ATR_MaintenanceTargetLabelDesc".Translate() + "\n\n" + "ATR_MaintenanceDesc".Translate() + ":\n\n" + maintenanceLevelInfoCached).Resolve();
        }

        // Randomly applies health defects based on random chances from the ticksSincePoorMaintenance level.
        public void TryPoorMaintenanceCheck()
        {
            Pawn_HealthTracker healthTracker = Pawn.health;
            float modifiedFailureTicks = Mathf.Abs(maintenanceEffectTicks) / ATReforged_Settings.maintenancePartFailureRateFactor;
            // Attempt to apply part decay.
            if (Rand.MTBEventOccurs(PartDecayContractChance.Evaluate(modifiedFailureTicks), TicksPerDay, 60f))
            {
                BodyPartRecord bodyPart = healthTracker.hediffSet.GetNotMissingParts()?.RandomElement();
                if (bodyPart == null)
                {
                    Log.Warning("[ATR] Attempted to apply part decay to " + Pawn + " but no viable body parts were found.");
                }
                else
                {
                    Hediff decayHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_PartDecay, Pawn, bodyPart);
                    healthTracker.AddHediff(decayHediff);
                    SendPartFailureLetter(Pawn, decayHediff);
                }
            }
            // Attempt to apply part rust.
            if (Rand.MTBEventOccurs(RustContractChance.Evaluate(modifiedFailureTicks), TicksPerDay, 60f))
            {
                BodyPartRecord bodyPart = healthTracker.hediffSet.GetNotMissingParts(depth: BodyPartDepth.Outside)?.RandomElement();
                if (bodyPart == null)
                {
                    Log.Warning("[ATR] Attempted to apply rust to " + Pawn + " but no viable body parts were found.");
                }
                else
                {
                    Hediff rustHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_RustedPart, Pawn, bodyPart);
                    healthTracker.AddHediff(rustHediff);
                    SendPartFailureLetter(Pawn, rustHediff);
                }
            }
            // Attempt to apply power loss.
            if (Rand.MTBEventOccurs(PowerLossContractChance.Evaluate(modifiedFailureTicks), TicksPerDay, 60f))
            {
                BodyPartRecord bodyPart = healthTracker.hediffSet.GetNotMissingParts()?.Where(part => part != healthTracker.hediffSet.GetBrain())?.RandomElement();
                if (bodyPart == null)
                {
                    Log.Warning("[ATR] Attempted to apply power loss to " + Pawn + " but no viable body parts were found.");
                }
                else
                {
                    Hediff powerLossHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_PowerLoss, Pawn, bodyPart);
                    healthTracker.AddHediff(powerLossHediff);
                    SendPartFailureLetter(Pawn, powerLossHediff);
                }
            }
            // Some effects can only be applied if the maintenance stage is critical.
            if (Stage == MaintenanceStage.Critical)
            {
                // Attempt to apply core damage.
                if (Rand.MTBEventOccurs(CoreDamageContractChance.Evaluate(modifiedFailureTicks), TicksPerDay, 60f))
                {
                    BodyPartRecord bodyPart = healthTracker.hediffSet.GetBrain();
                    if (bodyPart != null && healthTracker.capacities.GetLevel(PawnCapacityDefOf.Consciousness) > 0.3f)
                    {
                        Hediff coreDamageHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_DamagedCore, Pawn, bodyPart);
                        healthTracker.AddHediff(coreDamageHediff);
                        SendPartFailureLetter(Pawn, coreDamageHediff);
                    }
                }
                // Attempt to apply failing valves.
                if (Rand.MTBEventOccurs(FailingValvesContractChance.Evaluate(modifiedFailureTicks), TicksPerDay, 60f))
                {
                    BodyPartRecord bodyPart = Pawn.RaceProps.body.GetPartsWithDef(ATR_BodyPartDefOf.ATR_InternalCorePump)?.RandomElement();
                    if (bodyPart == null)
                    {
                        Log.Warning("[ATR] Attempted to apply failing valves to " + Pawn + " but no viable body parts were found.");
                    }
                    else
                    {
                        Hediff failingValvesHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_FailingCoolantValves, Pawn, bodyPart);
                        healthTracker.AddHediff(failingValvesHediff);
                        SendPartFailureLetter(Pawn, failingValvesHediff);
                    }
                }
                // Attempt to apply rogue mechanites.
                if (Rand.MTBEventOccurs(RogueMechanitesContractChance.Evaluate(modifiedFailureTicks), TicksPerDay, 60f))
                {
                    BodyPartRecord bodyPart = Pawn.RaceProps.body.GetPartsWithDef(ATR_BodyPartDefOf.ATR_MechaniteStorage)?.RandomElement();
                    if (bodyPart == null)
                    {
                        Log.Warning("[ATR] Attempted to apply rogue mechanites to " + Pawn + " but no viable body parts were found.");
                    }
                    else
                    {
                        Hediff rogueMechanitesHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_RogueMechanites, Pawn, bodyPart);
                        healthTracker.AddHediff(rogueMechanitesHediff);
                        SendPartFailureLetter(Pawn, rogueMechanitesHediff);
                    }
                }
            }
        }

        private float maintenanceLevel = -1;
        public float targetLevel = -1;
        private float cachedFallRatePerDay = 0.1f;
        public float maintenanceEffectTicks = TicksPerDay;
        public static string maintenanceLevelInfoCached;
    }
}