using RimWorld;
using Verse;

namespace ATReforged
{
    public class GenStep_DownedFractalWitness : GenStep_DownedRefugee
    {
        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            Pawn pawn;
            if (parms.sitePart != null && parms.sitePart.things != null && parms.sitePart.things.Any)
            {
                pawn = (Pawn)parms.sitePart.things.Take(parms.sitePart.things[0]);
            }
            else
            {
                DownedFractalWitnessComp component = map.Parent.GetComponent<DownedFractalWitnessComp>();
                if (component == null || !component.pawn.Any)
                {
                    pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(ATR_PawnKindDefOf.ATR_FractalWitness, Faction.OfAncients, PawnGenerationContext.NonPlayer, map.Tile, forceGenerateNewPawn: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, allowFood: false, allowAddictions: false));
                }
                else 
                {
                    pawn = component.pawn.Take(component.pawn[0]);
                }
            }

            // Give the witness a level 2 Psylink if Royalty is active.
            if (ModsConfig.RoyaltyActive)
            {
                Hediff_Level psylinkHediff = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_Level;
                pawn.health.AddHediff(psylinkHediff, pawn.health.hediffSet.GetBrain());
                psylinkHediff.SetLevelTo(2);
            }

            // Give the witness the Fractal hediff in the gifted stage.
            Hediff fractalHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_FractalPillOrganic, pawn);
            fractalHediff.Severity = 0.15f;
            pawn.health.AddHediff(fractalHediff);

            HealthUtility.DamageUntilDowned(pawn, false);
            GenSpawn.Spawn(pawn, loc, map);
            pawn.mindState.WillJoinColonyIfRescued = true;
            MapGenerator.rootsToUnfog.Add(loc);
            MapGenerator.SetVar("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
        }
    }
}