using RimWorld;
using Verse;

namespace ATReforged
{
    public class ThoughtWorker_MaintenanceMood : ThoughtWorker
    {
        public static ThoughtState CurrentThoughtState(Pawn p)
        {
            float maintenanceEffect = p.GetComp<CompMaintenanceNeed>().maintenanceEffectTicks;
            // Abysmal: 15 or more days of poor maintenance effect.
            if (maintenanceEffect < -900000)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            // Terrible: 9 or more days of poor maintenance effect.
            else if (maintenanceEffect < -540000)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            // Poor: 3 or more days of poor maintenance effect.
            else if (maintenanceEffect < -180000)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            // Standard: less than 3 days of poor or good maintenance effect.
            else if (maintenanceEffect < 180000)
            {
                return ThoughtState.ActiveAtStage(3);
            }
            // Decent: 3 or more days of good maintenance effect.
            else if (maintenanceEffect < 540000)
            {
                return ThoughtState.ActiveAtStage(4);
            }
            // Excellent: 9 or more days of good maintenance effect.
            else if (maintenanceEffect < 900000)
            {
                return ThoughtState.ActiveAtStage(5);
            }
            // Immaculate: 15 or more days of good maintenance effect.
            else
            {
                return ThoughtState.ActiveAtStage(6);
            }
        }

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (ThoughtUtility.ThoughtNullified(p, def) || p.GetComp<CompMaintenanceNeed>() == null)
            {
                return ThoughtState.Inactive;
            }
            return CurrentThoughtState(p);
        }
    }
}
