using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Job to attack anything and everything that isn't of the same def.
    public class JobGiver_Exterminator : JobGiver_Manhunter
    { 
        protected override Job TryGiveJob(Pawn sourcePawn)
        {
            bool attackExists = sourcePawn.TryGetAttackVerb(null, false) == null;
            Job result;
            if (!attackExists)
            {
                result = null;
            }
            else
            {
                Pawn targetPawn = FindPawnTarget(sourcePawn);
                if (targetPawn != null)
                {
                    result = MeleeAttackJob(sourcePawn, targetPawn);
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }
        
        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            return new Job(RimWorld.JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 999,
                expiryInterval = 999999,
                attackDoorIfTargetLost = true
            };
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing target) => target is Pawn targetPawn && targetPawn.def != pawn.def, 0f, 300f, default, float.MaxValue, true);
        }
    }
}
