using System.Collections.Generic;
using System.Text;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Alert_NeedMechanic : Alert
    {
        private List<Pawn> patientsResult = new List<Pawn>();

        private List<Pawn> Patients
        {
            get
            {
                patientsResult.Clear();
                foreach (Map map in Find.Maps)
                {
                    if (!map.IsPlayerHome)
                        continue;

                    bool hasMechanic = false;
                    foreach (Pawn colonist in map.mapPawns.FreeColonists)
                    {
                        if ((colonist.Spawned || colonist.BrieflyDespawned()) && !colonist.Downed && colonist.workSettings != null && colonist.workSettings.WorkIsActive(ATR_WorkTypeDefOf.ATR_Mechanic))
                        {
                            hasMechanic = true;
                            break;
                        }
                    }

                    if (hasMechanic)
                        continue;

                    List<Pawn> colonists = map.mapPawns.FreeColonists;
                    for (int i = colonists.Count - 1; i >= 0; i--)
                    {
                        Pawn colonist = colonists[i];
                        if ((colonist.Spawned || colonist.BrieflyDespawned()) && Utils.IsConsideredMechanical(colonist) && HealthAIUtility.ShouldBeTendedNowByPlayer(colonist))
                        {
                            patientsResult.Add(colonist);
                        }
                    }
                }

                return patientsResult;
            }
        }

        public Alert_NeedMechanic()
        {
            defaultLabel = "ATR_NeedMechanic".Translate();
            defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn patient in Patients)
            {
                stringBuilder.AppendLine("  - " + patient.NameShortColored.Resolve());
            }

            return "ATR_NeedMechanicDesc".Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            if (Find.AnyPlayerHomeMap == null)
            {
                return false;
            }

            return AlertReport.CulpritsAre(Patients);
        }
    }
}
