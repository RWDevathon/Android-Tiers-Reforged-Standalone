using Verse;
using RimWorld;
using System;

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

        // Fail-safe if an error should happen to occur at spawn, attempt to do it again repeatedly somewhat infrequently until success.
        public override void CompTick()
        {
            if (Find.TickManager.TicksGame % 339 == 0)
            {
                SpawnPawn();
                parent.Destroy();
            }
        }

        // Generate and spawn the created pawn.
        public void SpawnPawn()
        {
            try
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

                        // If this is the player's first constructed android, send a letter in case they don't understand how they work.
                        if (!Utils.gameComp.hasBuiltAndroid)
                        {
                            Find.LetterStack.ReceiveLetter("ATR_FirstBlankAndroidCreated".Translate(), "ATR_FirstBlankAndroidCreatedDesc".Translate(), LetterDefOf.NeutralEvent);
                            Utils.gameComp.hasBuiltAndroid = true;
                        }
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
                else if (Utils.IsConsideredMechanicalDrone(pawn) && !Utils.gameComp.hasBuiltDrone)
                {
                    Find.LetterStack.ReceiveLetter("ATR_FirstDroneCreated".Translate(), "ATR_FirstDroneCreatedDesc".Translate(), LetterDefOf.NeutralEvent);
                    Utils.gameComp.hasBuiltDrone = true;
                }

                GenSpawn.Spawn(pawn, parent.Position, parent.Map);
            }
            catch (Exception ex)
            {
                Log.Warning("[ATR] Error occured while generating/spawning a created pawn. This will leave a dummy Thing in its place! " + ex.Message + ex.StackTrace);
            }
        }
    }
}