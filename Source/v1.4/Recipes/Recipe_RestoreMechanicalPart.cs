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
            if (!Utils.IsConsideredMechanical(pawn))
            {
                yield break;
            }

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

            // Restore 100 points of severity for this part and child parts to normal functionality.
            float HPForRepair = 100;
            RestoreParts(pawn, part, ref HPForRepair);

            // If not all hp was used in a non-core part, then apply remaining hp to the Core part and its children.
            if (HPForRepair > 0 && part != pawn.def.race.body.corePart)
            {
                RestoreParts(pawn, pawn.def.race.body.corePart, ref HPForRepair);
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
        private void RestoreParts(Pawn pawn, BodyPartRecord part, ref float HPLeftToRestoreChildren)
        {
            if (part == null)
                return;

            // Acquire a list of all hediffs on this specific part, and prepare a bool to check if this part has hediffs that can't be handled with the available points.
            List<Hediff> targetHediffs = pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part && !hediff.def.keepOnBodyPartRestoration && hediff.def.isBad).ToList();

            // Destroy hediffs that does not put the HPLeft below 0. If there is any hediff with a severity too high, then recursion stops at this node.
            foreach (Hediff hediff in targetHediffs)
            {
                // If the Hediff has injuryProps, it's an injury whose severity matches the amount of lost HP.
                // If it does not have injuryProps, it's a disease or other condition whose severity is likely between 0 - 1 and should be adjusted to not be insignificant compared to injuries.
                float severity = hediff.Severity * (hediff.def.injuryProps == null ? 10 : 1);

                if (HPLeftToRestoreChildren < severity)
                {
                    // Injury severity can be reduced directly.
                    if (hediff.def.injuryProps != null)
                    {
                        hediff.Severity = -HPLeftToRestoreChildren;
                    }

                    // If the part is missing entirely and there is at least half the HP necessary to restore the part or half of the original HP unused, let it get away with it.
                    if (hediff.def == HediffDefOf.MissingBodyPart && (severity / 2 < HPLeftToRestoreChildren || HPLeftToRestoreChildren >= 50))
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                    return;
                }
                else
                {
                    HPLeftToRestoreChildren -= severity;
                    pawn.health.RemoveHediff(hediff);
                }
            }

            foreach (BodyPartRecord childPart in part.GetDirectChildParts())
            {
                RestoreParts(pawn, childPart, ref HPLeftToRestoreChildren);
            }
        }
    }
}