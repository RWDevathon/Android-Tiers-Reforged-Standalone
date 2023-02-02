using RimWorld;
using Verse.AI;
using Verse;

namespace ATReforged
{
    // Generate Insight work giver governs a pawn trying to generate skill, security, or hacking points at a research bench, as part of the Research work type.
    public class WorkGiver_GenerateInsight : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.ResearchBench);
            }
        }
        public override bool Prioritized => true;

        // If all point capacities are zero'd out, then there is no point in trying to identify an insight generation job as there is no capacity at all.
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (Utils.gameComp.GetPointCapacity(ServerType.SkillServer) <= 0 && Utils.gameComp.GetPointCapacity(ServerType.SecurityServer) <= 0 && Utils.gameComp.GetPointCapacity(ServerType.HackingServer) <= 0)
            {
                return true;
            }
            return false;
        }

        // Determine whether the pawn can have a insight generation job on the given work bench.
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_ResearchBench bench = t as Building_ResearchBench;
            if (bench == null)
            {
                return false;
            }

            // If this particular bench has no SkyMind connection (because it can't have one, or it isn't active), then no work can be done here.
            if (bench.GetComp<CompSkyMind>()?.connected != true)
            {
                return false;
            }

            // If this particular bench is set to a server type that is full on its points, then no work can be done here.
            CompInsightBench compInsightBench = bench.GetComp<CompInsightBench>();
            if (compInsightBench == null || Utils.gameComp.GetPointCapacity(compInsightBench.ServerType) <= Utils.gameComp.GetPoints(compInsightBench.ServerType))
            {
                return false;
            }
            // If the pawn can not reserve this particular bench, then no work can be done here.
            if (!pawn.CanReserve(t, 1, -1, null, forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(ATR_JobDefOf.ATR_GenerateInsight, t);
        }
    }
}
