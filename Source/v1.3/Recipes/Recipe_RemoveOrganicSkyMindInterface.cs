using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_RemoveOrganicSkyMindInterface : Recipe_RemoveImplant
    {
        // This operation is nearly identical to the vanilla RemoveImplant recipe, but makes sure to disconnect from the SkyMind before proceeding. Disconnects happen regardless of surgery outcome.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Utils.gameComp.DisconnectFromSkyMind(pawn);
            MedicalRecipesUtility.IsClean(pawn, part);
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
                {
                    return;
                }
                Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == recipe.removesHediff);
                if (hediff != null)
                {
                    if (hediff.def.spawnThingOnRemoved != null)
                    {
                        GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, billDoer.Position, billDoer.Map);
                    }
                    pawn.health.RemoveHediff(hediff);
                }
            }
            if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            {
                ReportViolation(pawn, billDoer, pawn.HomeFaction, -20);
            }
        }
    }
}