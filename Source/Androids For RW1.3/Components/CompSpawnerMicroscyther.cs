using Verse;
using RimWorld;

namespace ATReforged
{
    public class CompSpawnerMicroscyther : ThingComp
    {

        public override void CompTick()
        {
            SpawnPawn();
            parent.Destroy();
        }

        public void SpawnPawn()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.MicroScyther, Faction.OfAncientsHostile, PawnGenerationContext.NonPlayer);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(pawn, parent.Position, parent.Map);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ATR_MentalState_Exterminator, transitionSilently: true);
            
            Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.ATR_RemainingCharge, pawn, null);
            hediff.Severity = 0.5f;
            pawn.health.AddHediff(hediff, null, null);
        }
    }
}