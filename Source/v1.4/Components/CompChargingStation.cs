using System.Collections;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ATReforged
{
    public class CompChargingStation : ThingComp
    {
        // Generate adjacencies on start up so it doesn't have to regenerate it on each request.
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            adjacencies = GenAdj.CellsAdjacent8Way(parent);
        }

        // Return the first available spot on this station. Return IntVec3.Invalid if there is none.
        public IntVec3 GetOpenRechargeSpot(Pawn pawn)
        {
            foreach (IntVec3 adjPos in adjacencies)
            {
                if (pawn.CanReach(new LocalTargetInfo(adjPos), PathEndMode.OnCell, Danger.Deadly) && !pawn.Map.pawnDestinationReservationManager.IsReserved(adjPos))
                    return adjPos;
            }
            return IntVec3.Invalid;
        }

        private IEnumerable<IntVec3> adjacencies;
    }
}