using Verse;
using RimWorld;

namespace ATReforged
{
    [StaticConstructorOnStartup]
    public class MechFall : OrbitalStrike
    {
        public override void StartStrike()
        {
            base.StartStrike();
            MechFallMoteMaker.MakeMechFallMote(Position, Map);
        }

        public override void Tick()
        {
            if (TicksLeft == 0)
            {
                SpawnPawn();
            }
            else if (TicksLeft == 15)
            {
                CreateExplosion();
            }
            else if (TicksLeft == 160)
            {
                CreatePod();
            }
            base.Tick();
        }
        
        private void CreateExplosion()
        {
            GenExplosion.DoExplosion(Position, Map, 20, DamageDefOf.EMP, instigator, 200, -1f, null, weaponDef, def);
            GenExplosion.DoExplosion(Position, Map, 10, DamageDefOf.Bomb, instigator, 50, -1f, null, weaponDef, def, chanceToStartFire: 0.25f);
        }

        private void CreatePod()
        {
            ActiveDropPodInfo info = new ActiveDropPodInfo
            {
                openDelay = 10,
                leaveSlag = true
            };
            DropPodUtility.MakeDropPodAt(Position, Map, info);
        }

        public void SpawnPawn()
        {
            PawnKindDef landingMech;
            if (Rand.Chance(0.25f))
                landingMech = PawnKindDefOf.M8MechPawn;
            else
                landingMech = PawnKindDefOf.M7MechPawn;
            PawnGenerationRequest request = new PawnGenerationRequest(landingMech, Faction.OfPlayer, PawnGenerationContext.NonPlayer);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            FilthMaker.TryMakeFilth(Position, Map, RimWorld.ThingDefOf.Filth_RubbleBuilding, 30);

            // There is a very small chance the unit will be permanently hostile and try to murder everything it can find.
            if (Rand.Chance(0.05f))
            {
                pawn.SetFactionDirect(Faction.OfAncientsHostile);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ATR_MentalState_Exterminator, transitionSilently: true);

                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.ATR_RemainingCharge, pawn, null);
                hediff.Severity = 1f;
                pawn.health.AddHediff(hediff, null, null);
            }

            GenSpawn.Spawn(pawn, Position, Map);
        }
    }
}
