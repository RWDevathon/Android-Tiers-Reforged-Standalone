using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_InstallOrganicSkyMindInterface : Recipe_InstallImplant
    {
        // This recipe is specifically targetting organic brains, so we only need to check if the brain is available (a slight optimization over checking fixed body parts).
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            BodyPartRecord targetBodyPart = hediffSet.GetBrain();
            if (targetBodyPart != null && !Utils.IsConsideredMechanical(pawn))
            {
                // If the pawn has any implant that allows SkyMind connection already, then we can not install another one. SkyMind implants are mutually exclusive.
                for (int i = hediffSet.hediffs.Count - 1; i >= 0; i--)
                {
                    if (hediffSet.hediffs[i].TryGetComp<HediffComp_SkyMindEffecter>()?.AllowsSkyMindConnection == true)
                    {
                        yield break;
                    }
                }

                // If Biotech is active, ensure the pawn does not have a mechlink if we are applying a receiver (surrogates can not be mechanitors)
                if (ModLister.BiotechInstalled)
                {
                    if (recipe.addsHediff.CompProps<HediffCompProperties_SkyMindEffecter>().isReceiver && pawn.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant))
                    {
                        yield break;
                    }
                }
                yield return targetBodyPart;
            }
            yield break;
        }

        // Install the part as normal, and then handle which type of chip was installed if it was successful (which can be measured by seeing if it actually got the hediff or not).
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        { 
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            // If the pawn doesn't have the hediff, the operation failed. Also failed if they're dead now.
            if (pawn.Dead || !pawn.health.hediffSet.HasHediff(bill.recipe.addsHediff))
                return;

            // There are special considerations for adding these implants. Receiver chips kill the current mind.
            if (recipe.addsHediff.CompProps<HediffCompProperties_SkyMindEffecter>()?.isReceiver == true)
            {
                Utils.Duplicate(Utils.GetBlank(), pawn, isTethered: false);
                pawn.health.AddHediff(ATR_HediffDefOf.ATR_NoController);

                // If this is the pawn's first surrogate, send a letter with information about surrogates.
                if (!Utils.gameComp.hasMadeSurrogate)
                {
                    Find.LetterStack.ReceiveLetter("ATR_FirstSurrogateCreated".Translate(), "ATR_FirstSurrogateCreatedDesc".Translate(), LetterDefOf.NeutralEvent);
                    Utils.gameComp.hasMadeSurrogate = true;
                }
            }
        }
    }
}

