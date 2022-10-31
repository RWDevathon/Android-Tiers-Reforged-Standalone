using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace ATReforged
{
    // Target controller for using the Resurrection Kit
    public class CompTargetEffect_ResurrectMechanical : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            // Exit if the user is not a player controlled pawn or the pawn can not reserve/reach the intended target.
            if (user.Faction != Faction.OfPlayer || !user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
            {
                return;
            }
            Job job = new Job(JobDefOf.ResurrectMechanical, target, parent, user)
            {
                count = 1
            };
            user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}
