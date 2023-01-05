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
            PawnGenerationRequest request = new PawnGenerationRequest(Spawnprops.pawnKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true, canGeneratePawnRelations: false, allowFood: false, allowAddictions: false, fixedBiologicalAge: 0, fixedChronologicalAge: 0, fixedIdeo: null, forceNoIdeo: true, forceBaselinerChance: 1f);
            Pawn pawn = PawnGenerator.GeneratePawn(request);

            // Pawns may sometimes spawn with apparel somewhere in the generation process. Ensure they don't actually spawn with any - if they even can have apparel.
            pawn.apparel?.DestroyAll();

            // If the pawn is an android, it is spawned dormant without an installed intelligence. Animals are spawned pre-initialized.
            if (Utils.IsConsideredMechanicalAndroid(pawn))
            {
                Utils.Duplicate(Utils.GetBlank(), pawn, false, false);
                pawn.health.AddHediff(HediffDefOf.ATR_IsolatedCore, pawn.health.hediffSet.GetBrain());
                Hediff target = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_AutonomousCore);
                if (target != null)
                    pawn.health.RemoveHediff(target);
                pawn.playerSettings.medCare = MedicalCareCategory.NormalOrWorse;
                pawn.guest.SetGuestStatus(Faction.OfPlayer);
                Messages.Message("ATR_NewbootAndroidCreated".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            GenSpawn.Spawn(pawn, parent.Position, parent.Map);
        }
    }
}