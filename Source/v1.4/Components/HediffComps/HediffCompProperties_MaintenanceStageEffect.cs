using Verse;

namespace ATReforged
{
    public class HediffCompProperties_MaintenanceStageEffect : HediffCompProperties
    {
        public HediffCompProperties_MaintenanceStageEffect()
        {
            compClass = typeof(HediffComp_MaintenanceStageEffect);
        }

        public CompMaintenanceNeed.MaintenanceStage activeMaintenanceStage;
    }
}
