using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Recipe_SurgeryAndroids : RecipeWorker
    {
        private static readonly SimpleCurve KitMedicalPotencyToSurgeryChanceFactor = new SimpleCurve
        {
            new CurvePoint(0f, 0.7f),
            new CurvePoint(1f, 1f),
            new CurvePoint(2f, 1.3f)
        };

        protected bool CheckSurgeryFailAndroid(Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
        { // Check if the current surgery will fail.
            float chanceSucceed = 1f;
            // Multiply success chance by skill of surgeon (usually crafting capacity)
            if (!patient.RaceProps.IsMechanoid)
            {
                chanceSucceed *= surgeon.GetStatValue(ATR_StatDefOf.ATR_MechanicalSurgerySuccessChance);
            }

            // Multiply success chance by quality of bed
            if (!recipe.surgeryIgnoreEnvironment && patient.InBed())
            {
                chanceSucceed *= patient.CurrentBed().GetStatValue(ATR_StatDefOf.ATR_MechanicalSurgerySuccessChanceFactor);
            }
            else
            { // No bed? Reduce surgery success chance significantly.
                chanceSucceed *= 0.6f;
            }

            // Multiply by the tend quality of the kits used.
            chanceSucceed *= KitMedicalPotencyToSurgeryChanceFactor.Evaluate(GetAverageMedicalPotency(ingredients, bill));

            // Multiply success chance by the success chance of the surgery itself.
            chanceSucceed *= recipe.surgerySuccessChanceFactor;

            // If the surgeon has an inspiration for surgery, use it to massively increase success chance.
            if (surgeon.InspirationDef == InspirationDefOf.Inspired_Surgery && !patient.RaceProps.IsMechanoid)
            {
                if (chanceSucceed < 1f)
                {
                    chanceSucceed = 1f - (1f - chanceSucceed) * 0.1f;
                }
                surgeon.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Surgery);
            }

            // Max chance of success is either the calculated surgery chance or the settings-prescribed max limit (default 1).
            chanceSucceed = Mathf.Min(chanceSucceed, ATReforged_Settings.maxChanceMechanicOperationSuccess);

            // Check if the surgery is successful.
            if (ATReforged_Settings.showMechanicalSurgerySuccessChance)
                Messages.Message("[ATR Debug Utility] Surgery had " + chanceSucceed + " chance to succeed.", MessageTypeDefOf.NeutralEvent);
            if (!Rand.Chance(chanceSucceed))
            { // Surgery failed. Determine the extent of the failure.
                if (Rand.Chance(recipe.deathOnFailedSurgeryChance))
                { // Patient died due to the surgery. This kills them for good. 
                    HealthUtility.GiveRandomSurgeryInjuries(patient, 100, part);
                    patient.TakeDamage(new DamageInfo(DamageDefOf.Crush, 99999f, 999f, -1f, null, patient.health.hediffSet.GetBrain()));
                    if (!patient.Dead)
                    { // If they somehow didn't die from that, kill them with extreme force.
                        patient.Kill(null);
                    }
                    Messages.Message("MessageMedicalOperationFailureFatalAndroid".Translate(surgeon.LabelShort, patient.LabelShort, recipe.label), patient, MessageTypeDefOf.NegativeHealthEvent);
                }
                else if (Rand.Chance(ATReforged_Settings.chanceFailedOperationMinor))
                { // Patient suffered minor injuries.
                    Messages.Message("MessageMedicalOperationFailureMinorAndroid".Translate(surgeon.LabelShort, patient.LabelShort), patient, MessageTypeDefOf.NegativeHealthEvent);
                    HealthUtility.GiveRandomSurgeryInjuries(patient, 10, part);
                }
                else
                { // Patient suffered major injuries.
                    Messages.Message("MessageMedicalOperationFailureRidiculousAndroid".Translate(surgeon.LabelShort, patient.LabelShort), patient, MessageTypeDefOf.NegativeHealthEvent);
                    HealthUtility.GiveRandomSurgeryInjuries(patient, 65, part);
                }
                if (!patient.Dead)
                {
                    TryGainBotchedSurgeryThought(patient, surgeon);
                }
                return true;
            }
            return false;
        }

        // Apply a mood/opinion debuff to the victim of the botched surgery.
        private void TryGainBotchedSurgeryThought(Pawn patient, Pawn surgeon)
        {
            if (!patient.RaceProps.Humanlike)
            {
                return;
            }
            patient.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.BotchedMySurgery, surgeon);
        }

        // Taken directly from Vanilla calculations. Returns the average potency of used "medicine" or in this case, kits.
        private float GetAverageMedicalPotency(List<Thing> ingredients, Bill bill)
        {
            ThingDef thingDef = (bill as Bill_Medical)?.consumedInitialMedicineDef;
            int num = 0;
            float num2 = 0f;
            if (thingDef != null)
            {
                num++;
                num2 += thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency);
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                Medicine medicine = ingredients[i] as Medicine;
                if (medicine != null)
                {
                    num += medicine.stackCount;
                    num2 += medicine.GetStatValue(StatDefOf.MedicalPotency) * medicine.stackCount;
                }
            }

            if (num == 0)
            {
                return 1f;
            }

            return num2 / num;
        }
    }
}
