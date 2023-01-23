using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ATReforged
{
    // ThinkNode_ConditionalMustKeepLyingDown.Satisfied has a non-null checked rest conditional that will throw errors for mechanicals if not handled. This transpiler adds that null-check.
    internal class ThinkNode_ConditionalMustKeepLyingDown_Patch
    {
        [HarmonyPatch(typeof(ThinkNode_ConditionalMustKeepLyingDown))]
        [HarmonyPatch("Satisfied")]
        public class Satisfied_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                CodeInstruction startInstruction = new CodeInstruction(OpCodes.Ldarg_1);
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                int insertionPoint = -1;
                List<CodeInstruction> insertInstructions = new List<CodeInstruction>();
                Label insertLabelEnd = generator.DefineLabel();

                // Locate the first Need_Rest instance and insert a null check before it.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].Calls(AccessTools.PropertyGetter(typeof(Need), nameof(Need.CurLevel))))
                    {
                        // Mark the place where our instruction goes.
                        insertionPoint = i - 3;
                        // Ensure that any reference to this label ends up at our start instruction instead.
                        instructions[i - 3].MoveLabelsTo(startInstruction);
                        // Ensure that our branch out instruction ends up at this instruction.
                        instructions[i - 3].labels.Add(insertLabelEnd);
                        // Copy the two previous instructions so they match the exact need call stack values. Do this only once, as it will be the same for all calls.
                        if (insertInstructions.Count == 0)
                        {
                            for (int j = 2; j > 0; j--)
                            {
                                insertInstructions.Add(instructions[i - j]);
                            }
                        }
                        break;
                    }
                }

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    // Operation target hit, yield contained instructions and add null-check branch.
                    if (insertionPoint == i)
                    {
                        // If (pawn.Needs.Rest == null)
                        yield return startInstruction; // Load Pawn
                        foreach (CodeInstruction instruction in insertInstructions)
                        {
                            yield return instruction;
                        }
                        yield return new CodeInstruction(OpCodes.Ldnull); // Load a null
                        yield return new CodeInstruction(OpCodes.Ceq); // Compare Pawn.Needs.Rest to Null

                        yield return new CodeInstruction(OpCodes.Brfalse_S, insertLabelEnd); // Branch to next check if Pawn.Needs.Rest != null

                        yield return new CodeInstruction(OpCodes.Ldc_I4_0); // Load 0
                        yield return new CodeInstruction(OpCodes.Ret); // Return 0 (false)
                        yield return instructions[i]; // Return the instruction we encountered initially
                    }
                    // Not a target, return instruction as normal.
                    else
                    {
                        yield return instructions[i];
                    }
                }
            }
        }
    }
}