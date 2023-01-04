using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Job to attack anything and everything that isn't of the same def.
    public class JobGiver_Exterminator : ThinkNode_JobGiver
    { 
        protected override Job TryGiveJob(Pawn sourcePawn)
        {
            if (sourcePawn.TryGetAttackVerb(null) == null)
            {
                return null;
            }

            Pawn targetPawn = FindPawnTarget(sourcePawn);
            if (targetPawn != null)
            {
                return new Job(RimWorld.JobDefOf.AttackMelee, targetPawn)
                {
                    maxNumMeleeAttacks = 1,
                    expiryInterval = Rand.Range(420, 900),
                    attackDoorIfTargetLost = true,
                    canBashDoors = true,
                    canBashFences = true
                };
            }
            return null;
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing target) => target is Pawn targetPawn && targetPawn.def != pawn.def && targetPawn.Spawned && !targetPawn.Downed && !targetPawn.IsInvisible(), canBashDoors: true, canBashFences: true);
        }
    }
}
