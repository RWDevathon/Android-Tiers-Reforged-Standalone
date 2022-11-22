using Verse;
using RimWorld;

namespace ATReforged
{
    public class CompSpawnerGeneric : ThingComp
    {
        public CompProperties_GenericSpawner Spawnprops
        {
            get
            {
                return props as CompProperties_GenericSpawner;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            SpawnPawn();
            parent.Destroy();
        }

        // Generate and spawn the created pawn.
        public void SpawnPawn()
        { 
            PawnGenerationRequest request = new PawnGenerationRequest(Spawnprops.pawnKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true, canGeneratePawnRelations: false, allowFood: false, allowAddictions: false, fixedBiologicalAge: 0, fixedChronologicalAge: 0, forceNoIdeo: true);
            Pawn pawn = PawnGenerator.GeneratePawn(request);

            // If the pawn is an android, it is spawned dormant without an installed intelligence. Animals are spawned pre-initialized.
            if (Utils.IsConsideredMechanicalAndroid(pawn))
            {
                Utils.Duplicate(Utils.GetBlank(), pawn, false, false);
                Hediff target = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_AutonomousCore);
                if (target != null)
                    pawn.health.RemoveHediff(target);
                pawn.health.AddHediff(HediffDefOf.ATR_IsolatedCore, pawn.health.hediffSet.GetBrain());
                Messages.Message("ATR_NewbootAndroidCreated".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            GenSpawn.Spawn(pawn, parent.Position, parent.Map);
        }
    }
}