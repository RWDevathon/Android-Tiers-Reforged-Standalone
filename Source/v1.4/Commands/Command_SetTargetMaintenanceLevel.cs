using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ATReforged
{
    [StaticConstructorOnStartup]
    public class Command_SetTargetMaintenanceLevel : Command
    {
        public CompMaintenanceNeed maintenanceNeed;

        private List<CompMaintenanceNeed> maintenanceNeeds;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (maintenanceNeeds == null)
            {
                maintenanceNeeds = new List<CompMaintenanceNeed>();
            }

            if (!maintenanceNeeds.Contains(maintenanceNeed))
            {
                maintenanceNeeds.Add(maintenanceNeed);
            }

            int startingValue = 60;
            for (int j = 0; j < maintenanceNeeds.Count; j++)
            {
                if ((int)maintenanceNeeds[j].TargetMaintenanceLevel <= startingValue)
                {
                    startingValue = (int)(maintenanceNeeds[j].TargetMaintenanceLevel * 100);
                }
            }

            Dialog_Slider dialog_Slider = new Dialog_Slider((int x) => "ATR_SetTargetMaintenanceLevel".Translate(x), 0, 100, delegate (int value)
            {
                for (int k = 0; k < maintenanceNeeds.Count; k++)
                {
                    maintenanceNeeds[k].TargetMaintenanceLevel = value / 100f;
                }
            }, startingValue);

            Find.WindowStack.Add(dialog_Slider);
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (maintenanceNeeds == null)
            {
                maintenanceNeeds = new List<CompMaintenanceNeed>();
            }

            maintenanceNeeds.Add(((Command_SetTargetMaintenanceLevel)other).maintenanceNeed);
            return false;
        }
    }
}