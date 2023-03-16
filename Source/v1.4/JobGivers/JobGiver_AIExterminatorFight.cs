using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // The exterminator mental state drives pawns afflicted with it to fight enemies no matter how far away they are, and with no regards to safety or logic.
    public class JobGiver_AIExterminatorFight : JobGiver_AIFightEnemies
    {
        protected override Thing FindAttackTarget(Pawn pawn)
        {
            return (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedAutoTargetable, null, 0f, 9999f, default(IntVec3), float.MaxValue);
        }
    }
}
