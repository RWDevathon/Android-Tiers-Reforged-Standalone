using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // The exterminator mental state drives pawns afflicted with it to hunt relentlessly for targets, even through doors, across the map.
    public class JobGiver_AIExterminatorSap : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null) == null)
            {
                return null;
            }
            // Attempt to find a reachable pawn, allowing for passing doors.
            Pawn target = FindTargetPawnFor(pawn);
            if (target != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, target.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    Thing blocker = pawnPath.FirstBlockingBuilding(out IntVec3 cellBeforeBlock, pawn);
                    if (blocker != null)
                    {
                        return DigUtility.PassBlockerJob(pawn, blocker, cellBeforeBlock, false, true);
                    }
                }
            }
            // Attempt to find a valid target anywhere on the map, and sap towards them.
            target = (Pawn)GenClosest.ClosestThing_Global(pawn.Position, pawn.Map.mapPawns.AllPawnsSpawned, validator: delegate (Thing t) { return t.def != pawn.def; });
            if (target != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, target.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings)))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    Thing blocker = pawnPath.FirstBlockingBuilding(out IntVec3 cellBeforeBlock, pawn);
                    if (blocker != null)
                    {
                        return DigUtility.PassBlockerJob(pawn, blocker, cellBeforeBlock, false, true);
                    }
                }
            }
            return null;
        }

        private Pawn FindTargetPawnFor(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedAutoTargetable, (Thing x) => x is Pawn && pawn.HostileTo(x), 0f, 9999f, default(IntVec3), float.MaxValue, canBashDoors: true, canTakeTargetsCloserThanEffectiveMinRange: true, canBashFences: true);
        }
    }
}
