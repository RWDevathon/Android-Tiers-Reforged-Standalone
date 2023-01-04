using Verse;
using RimWorld;

namespace ATReforged
{
    public class Alert_LowSecurity : Alert
    {
        public Alert_LowSecurity()
        {
            defaultLabel = "ATR_AlertLowSecurity".Translate();
            defaultExplanation = "ATR_AlertLowSecurityDesc".Translate();
            defaultPriority = AlertPriority.High;
        }


        public override AlertReport GetReport()
        {
            if (!ATReforged_Settings.enemyHacksOccur || !Utils.gameComp.GetSkyMindDevices().Any())
                return false;

            float securityPoints = Utils.gameComp.GetPoints(ServerType.SecurityServer);

            // At peak wealth (1,000,000), the ratio is less than 250 wealth : 1 raid point. If the player does not meet this very gross underestimate that doesn't account for pawns, difficulty, or context, they are at severe risk.
            int simpleEstimatedWealthRaidPoints = (int)(Find.CurrentMap.PlayerWealthForStoryteller / 250);

            if (securityPoints < simpleEstimatedWealthRaidPoints * ATReforged_Settings.enemyHackAttackStrengthModifier)
            {
                return true;
            }
            return false;
        }
    }
}
