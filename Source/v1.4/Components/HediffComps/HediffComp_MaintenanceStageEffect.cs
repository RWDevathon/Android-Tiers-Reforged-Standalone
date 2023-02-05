using Verse;

namespace ATReforged
{
    public class HediffComp_MaintenanceStageEffect : HediffComp
    {
        public HediffCompProperties_MaintenanceStageEffect Props => (HediffCompProperties_MaintenanceStageEffect)props;

        public override bool CompShouldRemove => Pawn.GetComp<CompMaintenanceNeed>()?.Stage != Props.activeMaintenanceStage;
    }
}
