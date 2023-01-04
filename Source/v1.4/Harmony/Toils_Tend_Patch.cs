using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class Toils_Tend_Patch

    {
        // Ensure tending to mechanical units is tied to crafting, not medicine.
        [HarmonyPatch(typeof(Toils_Tend), "FinalizeTend")]
        public class FinalizeTend_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn patient, ref Toil __result)
            {
                if (Utils.IsConsideredMechanical(patient))
                {
                    Toil toil = new Toil();
                    toil.initAction = delegate
                    {
                        Pawn actor = toil.actor;
                        Medicine medicine = (Medicine)actor.CurJob.targetB.Thing;
                        float learnAmountBase = (!patient.RaceProps.Animal) ? 500f : 175f;
                        float learnMedicineFactor = (medicine != null) ? medicine.def.MedicineTendXpGainFactor : 0.5f;
                        actor.skills.Learn(SkillDefOf.Crafting, learnAmountBase * learnMedicineFactor);
                        TendUtility.DoTend(actor, patient, medicine);
                        if (medicine != null && medicine.Destroyed)
                        {
                            actor.CurJob.SetTarget(TargetIndex.B, LocalTargetInfo.Invalid);
                        }
                        if (toil.actor.CurJob.endAfterTendedOnce)
                        {
                            actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                        }
                    };
                    toil.defaultCompleteMode = ToilCompleteMode.Instant;
                    __result = toil;

                    return false;
                }
                return true;
            }
        }
    }
}