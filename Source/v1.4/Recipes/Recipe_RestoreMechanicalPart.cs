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
                // No reason to apply this to a part that has its parent missing. Restoring the parent would restore this part. IE. Why restore a finger when you can restore the hand?
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

            // Restore this part and all children to normal functionality.
            RestoreChildParts(pawn, part);
        }

        // Return an enumerable of all the missing or damaged body parts on this pawn.
        protected IEnumerable<BodyPartRecord> GetMissingOrDamagedParts(Pawn pawn)
        { 
            List<BodyPartRecord> allPartsList = pawn.def.race.body.AllParts;
            foreach (BodyPartRecord part in allPartsList)
            {
                if (pawn.health.hediffSet.PartIsMissing(part) || pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part && (hediff.def.tendable || hediff.def.chronic)).Any())
                {
                    yield return part;
                }
            }
        }

        // Recursively restore all parts that were missing that are children of the originally restored part. IE. hands and fingers when an arm was restored.
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