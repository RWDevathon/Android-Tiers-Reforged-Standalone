using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ATReforged
{
    // GatheringsUtility.ShouldGuestKeepAttendingGathering has a non-null checked food conditional that will throw errors for any humanlike pawn that does not eat food if not handled. This transpiler adds that null-check.
    internal class GatheringsUtility_Patch
    {
        [HarmonyPatch(typeof(GatheringsUtility))]
        [HarmonyPatch("ShouldGuestKeepAttendingGathering")]
        public class ShouldGuestKeepAttendingGathering_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                CodeInstruction startInstruction = new CodeInstruction(OpCodes.Ldarg_0);
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                List<CodeInstruction> insertInstructions = new List<CodeInstruction>();
                bool needNextLabel = false;
                int insertionPoint = -1;
                Label? insertLabelEnd = new Label?();

                // Locate the food check that we need to add a null check for, and handle the appropriate features.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].Calls(AccessTools.PropertyGetter(typeof(Need_Food), nameof(Need_Food.Starving))))
                    {
                        // Mark the place where our instruction goes.
                        insertionPoint = i - 3;
                        // Move any jumps over to our start instruction so they don't get skipped.
                        instructions[i - 3].MoveLabelsTo(startInstruction);
                        // Copy the two previous instructions so they match the exact need call stack values. Do this only once, as it will be the same for all calls.
                        if (insertInstructions.Count == 0)
                        {
                            for (int j = 2; j > 0; j--)
                            {
                                insertInstructions.Add(instructions[i - j]);
                            }
                        }
                        needNextLabel = true;
                    }
                    else if (needNextLabel && instructions[i].Branches(out insertLabelEnd))
                    {
                        break;
                    }
                }

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    // Operation target hit, yield contained instructions and add null-check branch.
                    if (insertionPoint == i)
                    {
                        // If (pawn.Needs.Food != null)
                        yield return startInstruction; // Load Pawn
                        foreach (CodeInstruction instruction in insertInstructions)
                        {
                            yield return instruction;
                        }
                        yield return new CodeInstruction(OpCodes.Brfalse_S, insertLabelEnd); // Branch to next check if Pawn.Needs.Rest == null
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