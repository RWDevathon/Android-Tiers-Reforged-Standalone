using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ATReforged
{
    /*
    // ThinkNode_ConditionalMustKeepLyingDown.Satisfied has a non-null checked rest conditional that will throw erors for mechanicals if not handled. This transpiler adds that null-check.
    internal class ThinkNode_ConditionalMustKeepLyingDown_Patch
    {
        [HarmonyPatch(typeof(ThinkNode_ConditionalMustKeepLyingDown))]
        [HarmonyPatch("Satisfied")]
        public class Satisfied_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                Log.Warning("[ATR DEBUG] Transpiler running!");
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                bool grabNextLabel = false;
                List<int> insertionPoints = new List<int>();
                List<CodeInstruction> insertInstructions = new List<CodeInstruction>();
                List<Label> insertionLabels = new List<Label>();

                // Locate all instances of the Rest need check, and store their location in the order and the label they branch to.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].Calls(AccessTools.PropertyGetter(typeof(Need), nameof(Need.CurLevel))))
                    {
                        Log.Warning("[ATR DEBUG] Target located! InsertionPoint should be " + (i - 3) + ".");
                        // Ensure our additional instruction occurs before the call and the instructions needed for the call.
                        insertionPoints.Add(i - 3);
                        // Copy the three previous instructions so they match the exact need call stack values. Do this only once, as it will be the same for all calls.
                        if (insertInstructions.Count == 0)
                        {
                            for (int j = 3; j > 0; j--)
                            {
                                Log.Warning("[ATR DEBUG] Caught pawn instruction opCode " + instructions[i - j].opcode + " with operand " + instructions[i - j].operand);
                                insertInstructions.Add(instructions[i - j]);
                            }
                        }
                        // Mark the next branch that occurs as possessing our desired label for jumping to if it is null.
                        grabNextLabel = true;
                    }
                    // If this instruction is the branch instruction we're looking for and we need to grab a label, the operand of this instruction is what we need.
                    else if (grabNextLabel && instructions[i].opcode == OpCodes.Ble_Un_S)
                    {
                        insertionLabels.Add((Label)instructions[i].operand);
                        grabNextLabel = false;
                    }
                }

                int insertionsMade = 0;
                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    // No remaining target instructions, skip the check until all instructions have been yielded.
                    if (insertionsMade > insertionPoints.Count - 1)
                    {
                        Log.Warning("[ATR DEBUG] No additional checks necessary.");
                        yield return instructions[i];
                        continue;
                    }
                    // Operation target hit, yield contained instructions and add null-check branch.
                    else if (insertionPoints[insertionsMade] == i)
                    {
                        Log.Warning("[ATR DEBUG] Successfully located target index " + i + " and inserting instructions into result.");
                        foreach (CodeInstruction instruction in insertInstructions)
                        {
                            yield return instruction;
                        }
                        yield return new CodeInstruction(OpCodes.Brfalse_S, insertionLabels[insertionsMade++]);
                        yield return instructions[i];
                    }
                    else
                    {
                        yield return instructions[i];
                    }
                }
            }
        }

        protected bool Satisfied(Pawn pawn)
        {
            if (pawn.CurJob == null || !pawn.GetPosture().Laying())
            {
                return false;
            }
            if (!pawn.Downed)
            {
                if (RestUtility.DisturbancePreventsLyingDown(pawn))
                {
                    return false;
                }
                if (!pawn.CurJob.restUntilHealed || !HealthAIUtility.ShouldSeekMedicalRest(pawn))
                {
                    if (!pawn.jobs.curDriver.asleep)
                    {
                        return false;
                    }
                    if (!pawn.CurJob.playerForced && RestUtility.TimetablePreventsLayDown(pawn))
                    {
                        return false;
                    }
                    if (pawn.needs.rest != null && pawn.needs.rest.CurLevel > 0.14f && ChildcareUtility.ShouldWakeUpToAutofeedUrgent(pawn))
                    {
                        return false;
                    }
                    if (pawn.needs.rest.CurLevel > 0.14f)
                    {
                        Need_Food food = pawn.needs.food;
                        if (food != null && (int)food.CurCategory >= 2 && FoodUtility.TryFindBestFoodSourceFor_NewTemp(pawn, pawn, desperate: false, out var _, out var _))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

    }
    */
}