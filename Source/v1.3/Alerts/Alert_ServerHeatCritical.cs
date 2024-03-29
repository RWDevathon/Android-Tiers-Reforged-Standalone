﻿using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

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

        public override AlertReport GetReport()
        {
            List<Thing> build = new List<Thing>();

            foreach (Thing thing in Utils.gameComp.GetHeatSensitiveDevices(Find.CurrentMap).Where(thing => thing.TryGetComp<CompHeatSensitive>().HeatLevel == 3))
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
