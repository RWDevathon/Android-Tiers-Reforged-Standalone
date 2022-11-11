using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    public class JobDriver_ResurrectMechanical : JobDriver
    {
        internal Corpse Corpse
        {
            get
            {
                return (Corpse)job.GetTarget(TargetIndex.A).Thing;
            }
        }

        private Thing Item
        {
            get
            {
                return job.GetTarget(TargetIndex.B).Thing;
            }
        }

        private Thing User
        {
            get
            {
                return job.GetTarget(TargetIndex.C).Thing;
            }
        }

        // Figure out if the pawn can reserve the corpse and the item. Return true if it can reserve both, false if not.
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        { 
            Pawn pawn = this.pawn;
            LocalTargetInfo target = Corpse;
            Job job = this.job;
            bool result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            if (result)
            {
                target = Item;
                result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            }
            return result;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            Toil prepare = Toils_General.Wait(1200, TargetIndex.None);
            prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedOrNull(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return prepare;
            yield return Toils_General.Do(new Action(Resurrect));
            yield break;
        }

        // Resurrect the targetted pawn if it is legal to do so.
        private void Resurrect()
        { 
            Pawn innerPawn = Corpse.InnerPawn;

            // Legal to resurrect pawns with this if they are considered mechanical and have a brain (core).
            if (Utils.IsConsideredMechanical(innerPawn) && innerPawn.health.hediffSet.GetBrain() != null)
            {
                // Drone Resurrection Kits may only resurrect drone units (simple-minded) or animal units.
                if (Item.def.defName == "ATR_DroneResurrectorKit" && Utils.IsConsideredMechanicalAndroid(innerPawn))
                {
                    Messages.Message("ATR_ResurrectionFailedDroneOnly".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.RejectInput, true);
                    return;
                }

                // Apply the long reboot (24 hours) to the pawn. This will ensure hostile units can be safely captured, and that your own units can't be reactivated mid-combat.
                Hediff rebootHediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("ATR_LongReboot"), innerPawn);
                innerPawn.health.AddHediff(rebootHediff);

                // This kit executes a full resurrection which removes all negative hediffs.
                ResurrectionUtility.Resurrect(innerPawn);

                // If the target is an android surrogate, then ensure it is booted up as a blank if a surrogate without any rebooting. Autonomous intact cores are fine as is.
                if (Utils.IsSurrogate(innerPawn))
                {
                    Hediff target = innerPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_ReceiverCore);
                    if (target != null)
                        innerPawn.health.RemoveHediff(target);

                    innerPawn.health.AddHediff(HediffDefOf.ATR_IsolatedCore);
                    innerPawn.health.RemoveHediff(rebootHediff);

                    // Dead surrogates of other factions are still be considered foreign. Remove that flag.
                    CompSkyMindLink targetComp = innerPawn.TryGetComp<CompSkyMindLink>();
                    if (targetComp.isForeign)
                        targetComp.isForeign = false;
                }

                // Make the revived pawn grateful to the pawn that revived them.
                if (innerPawn.needs.mood != null)
                {
                    innerPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RescuedMe, (Pawn)User);
                }

                // Notify successful resurrection and destroy the used kit.
                Messages.Message("ATR_ResurrectionSuccessful".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.PositiveEvent, true);
                Item.SplitOff(1).Destroy(DestroyMode.Vanish);
            }
            else
                Messages.Message("ATR_ResurrectionFailedInvalidPawn".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.RejectInput, true);
        }

        private const TargetIndex CorpseInd = TargetIndex.A;

        private const TargetIndex ItemInd = TargetIndex.B;

        private const int DurationTicks = 600;
    }
}
