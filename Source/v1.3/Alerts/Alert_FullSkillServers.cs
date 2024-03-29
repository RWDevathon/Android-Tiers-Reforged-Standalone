﻿using Verse;
using RimWorld;

namespace ATReforged
{
    public class Alert_FullSkillServers : Alert
    {
        public Alert_FullSkillServers()
        {
            defaultLabel = "ATR_AlertFullSkillServers".Translate();
            defaultExplanation = "ATR_AlertFullSkillServersDesc".Translate();
            defaultPriority = AlertPriority.Medium;
        }


        public override AlertReport GetReport()
        {
            if (!ATReforged_Settings.receiveSkillAlert || Utils.gameComp.GetSkillPointCapacity() <= 0)
                return false;

            if (Utils.gameComp.GetSkillPoints() >= Utils.gameComp.GetSkillPointCapacity() * 0.9f)
            {
                return true;
            }
            return false;
        }
    }
}
