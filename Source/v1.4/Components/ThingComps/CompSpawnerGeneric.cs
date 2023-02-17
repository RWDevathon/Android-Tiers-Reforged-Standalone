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

            // If the pawn is an android, special cases must be handled. Animals are spawned pre-initialized.
            if (Utils.IsConsideredMechanicalAndroid(pawn))
            {
                // Androids that need a Core to be functional are spawned blank and dormant.
                if (pawn.def.GetModExtension<ATR_MechTweaker>()?.needsCoreAsAndroid == true)
                {
                    Utils.Duplicate(Utils.GetBlank(), pawn, false, false);
                    pawn.health.AddHediff(ATR_HediffDefOf.ATR_IsolatedCore, pawn.health.hediffSet.GetBrain());
                    Hediff target = pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_AutonomousCore);
                    if (target != null)
                        pawn.health.RemoveHediff(target);
                    pawn.playerSettings.medCare = MedicalCareCategory.NormalOrWorse;
                    pawn.guest.SetGuestStatus(Faction.OfPlayer);
                    Messages.Message("ATR_NewbootAndroidCreated".Translate(), MessageTypeDefOf.PositiveEvent);
                }
                // Androids that do not need a Core are newly initialized, with the stats/passions of a 30-year old to avoid child debuff nonsense.
                else
                {
                    PawnGenerationRequest selfInitialized = new PawnGenerationRequest(Spawnprops.pawnKind, Faction.OfPlayer, forceGenerateNewPawn: true, canGeneratePawnRelations: false, allowAddictions: false, fixedBiologicalAge: 30, forceNoIdeo: true, colonistRelationChanceFactor: 0, forceBaselinerChance: 1f);
                    Pawn newPawn = PawnGenerator.GeneratePawn(selfInitialized);
                    Utils.Duplicate(newPawn, pawn, false, false);
                    Hediff rebootHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_LongReboot, pawn, null);
                    rebootHediff.Severity = 1;
                    pawn.health.AddHediff(rebootHediff);
                }
            }

            GenSpawn.Spawn(pawn, parent.Position, parent.Map);
        }
    }
}