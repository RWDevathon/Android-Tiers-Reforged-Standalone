using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_RestoreMechanicalPart : Recipe_SurgeryAndroids
    {
        // This surgery may be done on any missing, damaged, or defective part. Get the list of them and return it.
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        { 
            IEnumerable<BodyPartRecord> missingParts = GetMissingOrDamagedParts(pawn);
            foreach (BodyPartRecord part in missingParts)
            {
                // No reason to apply this to a part that has its parent missing or damaged. Restoring the parent would restore this part. IE. Why restore a finger when you can restore the hand?
                if (!missingParts.Contains(part.parent))
                    yield return part;
            }
        }

        // Restore the body part and all of its child parts. This surgery can not fail, and will never be treated as a violation of other faction pawns.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        { 
            if (billDoer != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }

            // Restore this part and 50 points of severity for children to normal functionality.
            float HPLeftForChildrenParts = 50 + part.def.hitPoints;
            RestoreChildParts(pawn, part, ref HPLeftForChildrenParts);

            // If not all hp was used in a non-core part, then apply remaining hp to the Core part and its children.
            if (HPLeftForChildrenParts > 0 && part != pawn.def.race.body.corePart)
            {
                RestoreChildParts(pawn, pawn.def.race.body.corePart, ref HPLeftForChildrenParts);
            }
        }

        // Return an enumerable of all the missing or damaged body parts on this pawn.
        protected IEnumerable<BodyPartRecord> GetMissingOrDamagedParts(Pawn pawn)
        { 
            List<BodyPartRecord> allPartsList = pawn.def.race.body.AllParts;
            foreach (BodyPartRecord part in allPartsList)
            {
                if (pawn.health.hediffSet.PartIsMissing(part) || pawn.health.hediffSet.hediffs.Any(hediff => hediff.Part == part && (hediff.def.tendable || hediff.def.chronic)))
                {
                    yield return part;
                }
            }
        }

        // Recursively restore children parts of the originally restored part. IE. hands and fingers when an arm was restored.
        private void RestoreChildParts(Pawn pawn, BodyPartRecord part, ref float HPLeftToRestoreChildren)
        {
            if (part == null)
                return;

            // Acquire a list of all hediffs on this specific part, and prepare a bool to check if this part has hediffs that can't be handled with the available points.
            List<Hediff> targetHediffs = pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part && !hediff.def.keepOnBodyPartRestoration && hediff.def.isBad).ToList();
            bool insufficientToCoverSeverity = false;

            // Destroy hediffs that does not put the HPLeft below 0. If there is any hediff with a severity too high, then recursion stops at this node.
            foreach (Hediff hediff in targetHediffs)
            {
                // If the Hediff has injuryProps, it's an injury whose severity matches the amount of lost HP.
                // If it does not have injuryProps, it's a disease or other condition whose severity is likely between 0 - 1 and should be adjusted to not be insignificant compared to injuries.
                float severity = hediff.Severity * (hediff.def.injuryProps == null ? 0 : 10);

                if (HPLeftToRestoreChildren < severity)
                {
                    insufficientToCoverSeverity = true;
                }
                else
                {
                    Log.Message("[ATR DEBUG] part " + part + " had hediff " + hediff.def.defName + " removed. Remaining HP is " + HPLeftToRestoreChildren);
                    HPLeftToRestoreChildren -= severity;
                    pawn.health.RemoveHediff(hediff);
                }
            }
            if (insufficientToCoverSeverity)
                return;
            foreach (BodyPartRecord childPart in part.GetDirectChildParts())
            {
                RestoreChildParts(pawn, childPart, ref HPLeftToRestoreChildren);
            }
        }
    }
}