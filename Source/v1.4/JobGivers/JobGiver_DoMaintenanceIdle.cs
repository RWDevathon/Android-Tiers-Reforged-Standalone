using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Mechanical units should do maintenance if they can't find anything else to do - this effectively replaces default vanilla's wandering behavior.
    public class JobGiver_DoMaintenanceIdle : ThinkNode_JobGiver
    {
        // Pawn ThinkTrees occasionally sort jobs to take on a priority. This is exceedingly low priority for idle maintenance.
        public override float GetPriority(Pawn pawn)
        {
            return 0.5f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompMaintenanceNeed compMaintenanceNeed = pawn.GetComp<CompMaintenanceNeed>();

            if (compMaintenanceNeed == null || pawn.InAggroMentalState || pawn.Downed || compMaintenanceNeed.MaintenanceLevel > 0.8f)
            {
                return null;
            }

            // If this pawn's current position is legal for maintenance, use it.
            if (cachedPawnMaintenanceSpots.ContainsKey(pawn.thingIDNumber))
            {
                Pair<IntVec3, int> cachedMaintenanceTimedSpot = cachedPawnMaintenanceSpots[pawn.thingIDNumber];
                if (cachedMaintenanceTimedSpot != null && cachedMaintenanceTimedSpot.First == pawn.Position && Find.TickManager.TicksGame - cachedMaintenanceTimedSpot.Second < 30000 && MaintenanceUtility.SafeEnvironmentalConditions(pawn, pawn.Position, pawn.Map) && pawn.CanReserveAndReach(pawn.Position, PathEndMode.OnCell, Danger.None))
                {
                    cachedPawnMaintenanceSpots[pawn.thingIDNumber] = new Pair<IntVec3, int>(pawn.Position, Find.TickManager.TicksGame);
                    return JobMaker.MakeJob(ATR_JobDefOf.ATR_DoMaintenanceIdle, pawn.Position, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(pawn.Position));
                }
            }

            // Find a valid place to do maintenance, and store it in the cache for later use.
            LocalTargetInfo maintenanceSpot = MaintenanceUtility.FindMaintenanceSpot(pawn);
            if (maintenanceSpot.IsValid)
            {
                cachedPawnMaintenanceSpots[pawn.thingIDNumber] = new Pair<IntVec3, int>(maintenanceSpot.Cell, Find.TickManager.TicksGame);
                return JobMaker.MakeJob(ATR_JobDefOf.ATR_DoMaintenanceIdle, maintenanceSpot.Cell, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(pawn.Position));
            }
            return null;
        }

        // A cached dictionary with Pawn ThingID keys and IntVec3,int Pair values matching a pawn's id to their last maintenance spot and when they started doing maintenance there.
        // If the stored tick for the last maintenance is less than 30,000 ticks old (1/2 of a day), reuse the spot instead of finding a new one.
        private static Dictionary<int, Pair<IntVec3, int>> cachedPawnMaintenanceSpots = new Dictionary<int, Pair<IntVec3, int>>();
    }
}
