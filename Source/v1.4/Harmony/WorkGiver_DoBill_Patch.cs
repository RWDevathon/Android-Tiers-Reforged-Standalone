using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;

namespace ATReforged
{
    internal class WorkGiver_DoBill_Patch
    {
        // Listen for doctors/mechanics doing a work bill, and make sure they select an appropriate medicine for their task.
        [HarmonyPatch(typeof(WorkGiver_DoBill), "AddEveryMedicineToRelevantThings")]
        public class AddEveryMedicineToRelevantThings_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing billGiver, ref List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map)
            { 
                try
                {
                    // If all medicines may be used for any operation, then no reason to remove any medicines from any operation.
                    if (!ATReforged_Settings.medicinesAreInterchangeable && billGiver is Pawn)
                    {
                        // If the patient is a mechanical unit, make sure to use a mechanical-compatible medicine (Reserved Repair Stims or additionally by settings)
                        if (Utils.IsConsideredMechanical(billGiver.def))
                        {
                            relevantThings.RemoveAll(thing => !Utils.IsMechanicalRepairStim(thing.def));
                        }
                        // If the patient is not mechanical, do not allow it to use Repair Stims. Other medicines will be handled by vanilla code.
                        else
                        {
                            relevantThings.RemoveAll(thing => Utils.IsMechanicalRepairStim(thing.def));
                        }
                    }
                }
                catch(Exception e)
                {
                    Log.Message("[ATR] WorkGiver_DoBill.AddEveryMedicineToRelevantThings " + e.Message + " " + e.StackTrace);
                }
            }
        }

        // Forbid doctors from working on mechanicals and mechanics from working on organics.
        [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
        public class JobOnThing_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing thing, bool forced, ref Job __result, WorkGiver_DoBill __instance)
            {
                if (__result == null)
                {
                    return;
                }
                else if (__instance.def.workType == WorkTypeDefOf.Doctor && thing is Pawn patient && Utils.IsConsideredMechanical(patient))
                {
                    __result = null;
                }
                else if (__instance.def.workType == ATR_WorkTypeDefOf.ATR_Mechanic && thing is Pawn unit && !Utils.IsConsideredMechanical(unit))
                {
                    __result = null;
                }
            }
        }
    }
}