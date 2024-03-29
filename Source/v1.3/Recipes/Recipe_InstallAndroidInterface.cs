﻿using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_InstallAndroidInterface : Recipe_InstallAndroidPart
    {
        // This recipe is specifically targetting the brain of a mechanical unit, so we only need to check if the brain is available (a slight optimization over checking fixed body parts).
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        { 

            BodyPartRecord targetBodyPart = pawn.health.hediffSet.GetBrain();
            if (targetBodyPart != null && (!Utils.IsConsideredMechanical(pawn) || pawn.health.hediffSet.hediffs.Where(hediff => hediff.def == HediffDefOf.ATR_IsolatedCore).Any()))
            {
                yield return targetBodyPart;
            }
            yield break;
        }

        // Install the part as normal, and then handle which type of core was installed if it was successful (which can be measured by seeing if it actually got the hediff or not).
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        { 
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            // If the pawn doesn't have any non-isolated core hediff, the operation failed. Also failed if they're dead now.
            if (pawn.Dead || !pawn.health.hediffSet.hediffs.Where(hediff => hediff.def == HediffDefOf.ATR_AutonomousCore || hediff.def == HediffDefOf.ATR_ReceiverCore).Any())
                return;

            // There are special considerations for adding the core (brain) itself. Adding a core makes a new intelligence. Receiver core initializes it as a surrogate. Adding any core removes the "Isolated Core" hediff.
            // Initializing a new android. Create the new intelligence.
            if (recipe.addsHediff == HediffDefOf.ATR_AutonomousCore)
            {
                /*
                PawnGenerationRequest request = new PawnGenerationRequest(pawn.kindDef, Faction.OfPlayer, canGeneratePawnRelations: false, colonistRelationChanceFactor: 0, fixedGender: pawn.gender);
                Pawn newIntelligence = PawnGenerator.GeneratePawn(request);
                Utils.Duplicate(newIntelligence, pawn, false, false);
                */
                Find.WindowStack.Add(new Dialog_InitializeMind(pawn));
                pawn.health.AddHediff(HediffDefOf.ATR_LongReboot);
            }
            // Initializing a surrogate. Ensure surrogate details are initialized properly.
            else
            {
                pawn.health.AddHediff(HediffDefOf.ATR_NoController);
            }

            // Remove the isolated core hediff and the reboot hediff (Androids get Long Reboot and Surrogates get nothing). 
            Hediff target = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_IsolatedCore);
            if (target != null)
            {
                pawn.health.RemoveHediff(target);
            }
            target = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_ShortReboot);
            if (target != null)
            {
                pawn.health.RemoveHediff(target);
            }

        }
    }
}

