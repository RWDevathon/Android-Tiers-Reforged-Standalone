using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_InstallAndroidPart : Recipe_SurgeryAndroids
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        { // Acquire a the list of viable body parts to attach the given recipe to, checking to ensure they are in fact intact and ready for surgery.
            List<BodyPartRecord> pawnParts = pawn.RaceProps.body.AllParts;
            List<BodyPartDef> targetParts = recipe.appliedOnFixedBodyParts;
            foreach (BodyPartRecord part in pawnParts)
            {
                // If this part is a target part, then check if it's capable of receiving surgery.
                if (targetParts.Contains(part.def))
                {
                    // Acquire all hediffs relating to this part.
                    IEnumerable<Hediff> diffs = from hediff in pawn.health.hediffSet.hediffs
                                                where hediff.Part == part
                                                select hediff;

                    // Hediffs don't need to be applied to a part that has 1 hediff that is the exact same as this recipe's hediff.
                    if (diffs.Count() != 1 || diffs.First().def != recipe.addsHediff)
                    {
                        // Check if the parent part of this part is nonexistant or is not missing.
                        if (part.parent == null || pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).Contains(part.parent))
                        {
                            // Can't add part to something that has an ancestor that is already replaced by a whole new part (ie. can't add a new hand when there's already a whole new arm).
                            if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                            {
                                yield return part;
                            }
                        }
                    }
                }
            }
            yield break;
        }

        // Check if the surgery fails. If it doesn't, then apply the hediff.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        { 
            // Mechanical units must undergo a short reboot on all installations.
            pawn.health.AddHediff(HediffDefOf.ATR_ShortReboot);
            if (billDoer != null)
            {
                if (CheckSurgeryFailAndroid(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map);
            }
            else if (pawn.Map != null)
            {
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, pawn.Position, pawn.Map);
            }
            else
            {
                pawn.health.RestorePart(part);
            }
            pawn.health.AddHediff(recipe.addsHediff, part, null);
        }
    }
}

