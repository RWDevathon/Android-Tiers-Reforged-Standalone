using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_RemoveMechanicalPart : Recipe_SurgeryAndroids
    {

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (!Utils.IsConsideredMechanical(pawn))
            {
                yield break;
            }

            IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts();
            foreach (BodyPartRecord part in notMissingParts)
            {
                if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
                {
                    yield return part;
                }
                else if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
                {
                    yield return part;
                }
                else if (part != pawn.RaceProps.body.corePart && part.def.canSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part))
                {
                    yield return part;
                }
                else if (part.def.forceAlwaysRemovable)
                {
                    yield return part;
                }
            }
        }

        // Remove the body part from the pawn after the surgery is complete. Also handle returning the part itself and restoring any body parts.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                bool removingInterface = false;
                bool isAutonomousIntelligence = false;
                // Check to see if the procedure is removing a core interface. If it is, then we handle special considerations later. This check must happen before the part is removed.
                if (part == pawn.health.hediffSet.GetBrain())
                {
                    removingInterface = true;
                    isAutonomousIntelligence = pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_AutonomousCore) != null;
                }

                // Check if the surgery failed. If it did, exit early.
                if (CheckSurgeryFailAndroid(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }

                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

                if (pawn.health.hediffSet.GetNotMissingParts().Contains(part))
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == part))
                    {
                        if (hediff.def.spawnThingOnRemoved != null)
                        {
                            GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, billDoer.Position, billDoer.Map);
                        }
                    }
                }

                // If the removed part represented the entire body part, then removing it would normally leave the part destroyed. Instead, restore this part and all children to normal functionality.
                if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part).ToList())
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                    RestoreChildParts(pawn, part);
                }

                // If removing the part represents a violation of a foreign pawn's trust, then report it.
                if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
                {
                    ReportViolation(pawn, billDoer, pawn.HomeFaction, -40);
                }

                // There are special considerations for removing the core (brain) itself. Removing an autonomous core is murder. Removing any core applies the "Isolated Core" hediff and removes the SkyMind comp.
                if (removingInterface)
                {
                    // Removing an interface always disconnects a pawn from the SkyMind network. This needs to disconnect surrogates, controllers, and those currently in mind operations.
                    // This will ensure all appropriate comps and interactions are taken care of before continuing
                    Utils.gameComp.DisconnectFromSkyMind(pawn);

                    // Surrogates are already handled via disconnecting from the SkyMind or are already blank. Autonomous intelligences must be murdered and made blank.
                    if (isAutonomousIntelligence)
                    {
                        Utils.Duplicate(Utils.GetBlank(), pawn, isTethered: false);
                        pawn.guest?.SetGuestStatus(Faction.OfPlayer);
                        if (pawn.playerSettings != null)
                            pawn.playerSettings.medCare = MedicalCareCategory.Best;

                        // Send a message about the removed intelligence
                        Messages.Message("ATR_InterfaceRemoved".Translate(), MessageTypeDefOf.NegativeEvent);
                    }

                    // Clean up and apply the appropriate hediff. Apply Isolated core before no host is applied to ensure the pawn doesn't become capable of moving for a tick.
                    pawn.health.AddHediff(ATR_HediffDefOf.ATR_IsolatedCore, pawn.health.hediffSet.GetBrain());
                    Hediff targetHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_NoController);
                    if (targetHediff != null)
                        pawn.health.RemoveHediff(targetHediff);
                }
            }
        }

        // Recursively restore all parts that were missing as a result of the added body part. IE. hands and fingers when an arm upgrade was removed.
        private void RestoreChildParts(Pawn pawn, BodyPartRecord part)
        { 
            if (part == null)
                return;
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part && !hediff.def.keepOnBodyPartRestoration && hediff.def.isBad).ToList())
            {
                pawn.health.RemoveHediff(hediff);
            }
            foreach (BodyPartRecord childPart in part.GetDirectChildParts())
            {
                RestoreChildParts(pawn, childPart);
            }
        }
    }
}