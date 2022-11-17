using System.Collections.Generic;
using System.Text;
using Verse;
using RimWorld;
using System.Linq;

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
                        if ((colonist.Spawned || colonist.BrieflyDespawned()) && !colonist.Downed && colonist.workSettings != null && colonist.workSettings.WorkIsActive(WorkTypeDefOf.Mechanic))
                        {
                            hasMechanic = true;
                            break;
                        }
                    }

                    if (hasMechanic)
                        continue;

                    patientsResult.AddRange(map.mapPawns.FreeColonists.Where(pawn => (pawn.Spawned || pawn.BrieflyDespawned()) && Utils.IsConsideredMechanical(pawn) && HealthAIUtility.ShouldBeTendedNowByPlayer(pawn)));
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
