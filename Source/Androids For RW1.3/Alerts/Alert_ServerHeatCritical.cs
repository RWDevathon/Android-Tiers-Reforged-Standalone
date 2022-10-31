using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace ATReforged
{
    public class Alert_HeatCritical : Alert_Critical
    {
        public Alert_HeatCritical()
        {
            defaultLabel = "ATPP_AlertServerHeatCritical".Translate();
            defaultExplanation = "ATPP_AlertServerHeatCriticalDesc".Translate();
            defaultPriority = AlertPriority.Critical;
        }

        public override AlertReport GetReport()
        {
            List<Thing> build = new List<Thing>();

            foreach (Thing thing in Utils.GCATPP.GetHeatSensitiveDevices(Find.CurrentMap).Where(thing => thing.TryGetComp<CompHeatSensitive>().HeatLevel == 3))
            {
                build.Add(thing);
            }

            if (build != null)
                return AlertReport.CulpritsAre(build);
            else
                return false;
        }
    }
}
