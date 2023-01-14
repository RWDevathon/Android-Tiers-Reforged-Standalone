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
                    IEnumerable<Hediff> diffs = pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part);

                    // Hediffs don't need to be applied to a part that has 1 hediff that is the exact same as this recipe's hediff.
                    if (diffs.Count() != 1 || diffs.First().def != recipe.addsHediff)
                    {
                        // Check if the parent part of this part is nonexistant or is not missing.
                        if (part.parent == null || pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).Contains(part.parent))
                        {
                            // Can't add part to something that has an ancestor that is already replaced by a whole new part (ie. can't add a new hand when there's already a whole new arm).
                            // Also can't add a part to a part that is kept when restored (it must be removed first!)
                            if (!pawn.health.hediffSet.AncestorHasDirectlyAddedParts(part) && !diffs.Any(hediff => hediff.def.keepOnBodyPartRestoration))
                            {
                                yield return part;
                            }
                        }
                    }
                }
            }
            yield break;
        }

        // Attempt to apply the appropriate hediff if the operation succeeds. Also track violations and giving back any already existing parts that are replaced.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        { 
            // Mechanical units must undergo a short reboot on all installations.
            pawn.health.AddHediff(ATR_HediffDefOf.ATR_ShortReboot);
            bool isViolation = !PawnGenerator.IsBeingGenerated(pawn) && IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (CheckSurgeryFailAndroid(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

                if (MedicalRecipesUtility.IsClean(pawn, part) && isViolation && part.def.spawnThingOnRemoved != null)
                {
                    ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
                }

                if (isViolation)
                {
                    ReportViolation(pawn, billDoer, pawn.HomeFaction, -40);
                }

                if (ModsConfig.IdeologyActive)
                {
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InstalledProsthetic, billDoer.Named(HistoryEventArgsNames.Doer)));
                }
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
            pawn.health.AddHediff(recipe.addsHediff, part);
        }
    }
}

