using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Alert_HeatCritical : Alert_Critical
    {
        public Alert_HeatCritical()
        {
            defaultLabel = "ATR_AlertServerHeatCritical".Translate();
            defaultExplanation = "ATR_AlertServerHeatCriticalDesc".Translate();
            defaultPriority = AlertPriority.Critical;
        }

        private List<Thing> cachedBuildings = new List<Thing>();

        List<Thing> CachedBuildings 
        {
            get
            {
                cachedBuildings.Clear();
                List<Thing> things = Utils.gameComp.GetHeatSensitiveDevices();
                for (int i = things.Count - 1; i > -1; i--)
                {
                    if (things[i].TryGetComp<CompHeatSensitive>()?.HeatLevel == 3)
                    {
                        cachedBuildings.Add(things[i]);
                    }
                }
                return cachedBuildings;
            }
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(CachedBuildings);
        }
    }
}
