using RimWorld;
using Verse;

namespace ATReforged
{
    public class GenStep_DownedT5Android : GenStep_DownedRefugee
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
                DownedT5AndroidComp component = map.Parent.GetComponent<DownedT5AndroidComp>();
                if (component == null || !component.pawn.Any)
                {
                    pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(ATR_PawnKindDefOf.ATR_T5Colonist, Faction.OfAncients, PawnGenerationContext.NonPlayer, map.Tile, forceGenerateNewPawn: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, allowFood: false, allowAddictions: false));
                }
                else 
                {
                    pawn = component.pawn.Take(component.pawn[0]);
                }
            }
            Hediff hediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_LongReboot, pawn, null);
            hediff.Severity = 1f;
            pawn.health.AddHediff(hediff);
            GenSpawn.Spawn(pawn, loc, map);
            pawn.mindState.WillJoinColonyIfRescued = true;
            MapGenerator.rootsToUnfog.Add(loc);
            MapGenerator.SetVar("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
        }
    }
}