using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ATReforged
{
    /* TODO: Destroy Need_Maintenance when proven unnecessary.
    public class Need_Maintenance : Need
    {
        public enum MaintenanceCategories
        {
            Critical,
            Poor,
            Sufficient,
            Satisfactory
        }

        private bool PawnNeedsMaintenance => Utils.IsConsideredMechanical(pawn) && pawn.def.GetModExtension<ATR_MechTweaker>()?.needsMaintenance == true;

        private float PawnMaintenanceEfficiency => pawn.GetStatValue(StatDefOf.ATR_MaintenanceRetention);

        private const float ThresholdCritical = 0.2f;

        private const float ThresholdPoor = 0.35f;

        private const float ThresholdSatisfactory = 0.8f;

        private const float TicksPerDay = 60000f;

        private const float TicksPerInterval = 150f;

        private float DailyFallPerCategory()
        {
            switch (CurrentLevelCategory)
            {
                case MaintenanceCategories.Critical:
                    return 0.03f;
                case MaintenanceCategories.Poor:
                    return 0.04f;
                case MaintenanceCategories.Sufficient:
                    return 0.05f;
                default:
                    return 0.2f;
            }
        }

        private float FallPerNeedIntervalTick => TicksPerInterval * DailyFallPerCategory() / TicksPerDay;

        public MaintenanceCategories CurrentLevelCategory
        {
            get
            {
                if (Disabled)
                {
                    return MaintenanceCategories.Sufficient;
                }
                float curLevel = CurLevel;
                if (curLevel <= ThresholdCritical)
                {
                    return MaintenanceCategories.Critical;
                }
                if (curLevel <= ThresholdPoor)
                {
                    return MaintenanceCategories.Poor;
                }
                if (curLevel <= ThresholdSatisfactory)
                {
                    return MaintenanceCategories.Sufficient;
                }
                return MaintenanceCategories.Satisfactory;
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (pawn.CurJobDef == JobDefOf.ATR_DoMaintenance)
                {

                }
                return isBeingMaintained ? 1 : -1;
            }
        }

        public override bool ShowOnNeedList => !Disabled;

        private bool Disabled => !PawnNeedsMaintenance;

        public void Notify_MaintenanceApplied(float maintenanceProvided)
        {
            if (!Disabled)
            {
                CurLevel += maintenanceProvided;
            }
        }

        public Need_Maintenance(Pawn pawn) : base(pawn)
        {
        }

        public override void SetInitialLevel()
        {
            CurLevelPercentage = 0.6f;
        }

        public override void NeedInterval()
        {
            if (Disabled)
            {
                SetInitialLevel();
            }

            if (!isBeingMaintained)
            {
                CurLevel -= FallPerNeedIntervalTick / PawnMaintenanceEfficiency;
            }
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
        {
            if (threshPercents == null)
            {
                threshPercents = new List<float>();
            }
            threshPercents.Clear();
            threshPercents.Add(ThresholdCritical);
            threshPercents.Add(ThresholdPoor);
            threshPercents.Add(ThresholdSatisfactory);
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
        }
    }
    */
}