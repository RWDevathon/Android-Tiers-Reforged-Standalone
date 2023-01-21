using Verse;
using RimWorld;

namespace ATReforged
{
    public class CompSpawnerMicroScyther : ThingComp
    {

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            SpawnPawn();
            parent.Destroy();
        }

        public void SpawnPawn()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(ATR_PawnKindDefOf.ATR_MicroScyther, Faction.OfAncientsHostile, PawnGenerationContext.NonPlayer);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(pawn, parent.Position, parent.Map);
            pawn.mindState.mentalStateHandler.TryStartMentalState(ATR_MentalStateDefOf.ATR_MentalState_Exterminator, transitionSilently: true);
            
            Hediff hediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_RemainingCharge, pawn, null);
            hediff.Severity = 0.5f;
            pawn.health.AddHediff(hediff, null, null);
        }
    }
}