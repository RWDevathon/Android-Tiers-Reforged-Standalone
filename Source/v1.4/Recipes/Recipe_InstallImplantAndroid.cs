using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_InstallImplantAndroid : Recipe_SurgeryAndroids
    {
        // Acquire a the list of viable body parts to attach the given recipe to, checking to ensure they are intact and ready for surgery.
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
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

                    // Check that the part is not missing.
                    if (pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).Contains(part))
                    {
                        // Check that the part or any ancestors are replaced by a whole new part.
                        if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                        {
                            // Check that the hediff to apply is not already on the part.
                            if (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == part && x.def == recipe.addsHediff))
                            {
                                yield return part;
                            }
                        }
                    }
                }
            }
            yield break;
        }

        // Check if the surgery fails. If not, then apply the appropriate hediff.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
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
            }
            pawn.health.AddHediff(recipe.addsHediff, part);
        }
    }
}
