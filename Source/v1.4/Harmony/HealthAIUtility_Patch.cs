using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;

namespace ATReforged
{
    internal class HealthAIUtility_Patch
    {
        // Ensure the appropriate care is selected for organics and mechanicals if medicinesAreInterchangeable is false in settings. Repair stims for mechanicals, medicine for organics.
        [HarmonyPatch(typeof(HealthAIUtility), "FindBestMedicine")]
        public class FindBestMedicine_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn healer, Pawn patient, ref Thing __result)
            {
                if (ATReforged_Settings.medicinesAreInterchangeable)
                    return;

                if (patient.playerSettings == null || patient.playerSettings.medCare <= MedicalCareCategory.NoMeds)
                {
                    __result = null;
                    return;
                }
                if (Medicine.GetMedicineCountToFullyHeal(patient) <= 0)
                {
                    __result = null;
                    return;
                }

                float medicalPotency = 0;
                if(__result != null)
                {
                    medicalPotency = __result.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
                }

                Predicate<Thing> validator;
                if (Utils.IsConsideredMechanical(patient))
                {
                    validator = (Thing medicine) => Utils.IsMechanicalRepairStim(medicine.def) && medicine.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null) <= medicalPotency && !medicine.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(medicine.def) && healer.CanReserve(medicine, 10, 1);
                }
                else
                {
                    validator = (Thing medicine) => !Utils.IsMechanicalRepairStim(medicine.def) && medicine.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null) <= medicalPotency && !medicine.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(medicine.def) && healer.CanReserve(medicine, 10, 1);
                }


                IntVec3 position = patient.Position;
                Map map = patient.Map;
                List<Thing> searchSet = patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
                PathEndMode peMode = PathEndMode.ClosestTouch;
                TraverseParms traverseParams = TraverseParms.For(healer, Danger.Deadly, TraverseMode.ByPawn, false);
                __result = GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator, (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null));
            }
        }
    }
}