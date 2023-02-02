using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ATReforged
{
    // A simple CompAssignable subclass that allows mechanical pawns to be assigned to a spot. There are no restrictions besides having a maintenance need (no 1-1 restrictions, etc).
    public class CompAssignableToPawn_MaintenanceSpot : CompAssignableToPawn
    {
        // All animals and colonists are possible candidates (with the exception of Dryads, which Core seems to think have a special use case).
        public override IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!parent.Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }
                List<Pawn> candidates = new List<Pawn>();
                foreach (Pawn pawn in parent.Map.mapPawns.SpawnedColonyAnimals)
                {
                    if (!pawn.RaceProps.Dryad)
                    {
                        candidates.Add(pawn);
                    }
                }
                foreach (Pawn pawn in parent.Map.mapPawns.FreeColonists)
                {
                    candidates.Add(pawn);
                }
                return candidates.OrderByDescending(delegate (Pawn p)
                {
                    if (!CanAssignTo(p).Accepted)
                    {
                        return 0;
                    }
                    return 1;
                }).ThenBy((Pawn p) => p.LabelShort);
            }
        }

        // Can be assigned if the pawn has a maintenance need.
        public override AcceptanceReport CanAssignTo(Pawn pawn)
        {
            if (pawn.GetComp<CompMaintenanceNeed>() == null)
            {
                return "ATR_MaintenanceNeedRequired".Translate();
            }
            return AcceptanceReport.WasAccepted;
        }
    }
}
