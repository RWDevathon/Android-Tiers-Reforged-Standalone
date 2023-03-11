using Verse;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld;
using System.Collections.Generic;
using Verse.Grammar;

namespace ATReforged
{
    public class SitePartWorker_WitnessArrival : SitePartWorker_DownedRefugee
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(ATR_PawnKindDefOf.ATR_FractalWitness, Faction.OfAncients, PawnGenerationContext.NonPlayer, part.site.Tile, forceGenerateNewPawn: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, allowFood: false, allowAddictions: false));

            part.things = new ThingOwner<Pawn>(part, oneStackOnly: true);
            part.things.TryAdd(pawn);
            if (pawn.relations != null)
            {
                pawn.relations.everSeenByPlayer = true;
            }

            Pawn mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
            if (mostImportantColonyRelative != null)
            {
                PawnRelationDef mostImportantRelation = mostImportantColonyRelative.GetMostImportantRelation(pawn);
                TaggedString text = "";
                if (mostImportantRelation != null && mostImportantRelation.opinionOffset > 0)
                {
                    pawn.relations.relativeInvolvedInRescueQuest = mostImportantColonyRelative;
                    text = "\n\n" + "RelatedPawnInvolvedInQuest".Translate(mostImportantColonyRelative.LabelShort, mostImportantRelation.GetGenderSpecificLabel(pawn), mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn);
                }
                else
                {
                    PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
                }

                outExtraDescriptionRules.Add(new Rule_String("pawnInvolvedInQuestInfo", text));
            }

            slate.Set("witness", pawn);
        }
    }
}