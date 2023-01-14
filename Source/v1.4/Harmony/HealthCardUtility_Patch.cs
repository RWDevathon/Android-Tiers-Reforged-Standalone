using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using System.Reflection;

namespace ATReforged
{
    // Vanilla Self-tending can not be enabled for player pawns if the doctor work type is completely disabled. This transpiler makes it so that mechanical units check against Mechanic instead.
    internal class HealthCardUtility_Patch
    {
        [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
        public class DrawOverviewTab_Patch
        {
            [HarmonyPrepare]
            public static bool Prepare() => DefDatabase<WorkTypeDef>.AllDefsListForReading.Count > 0;

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                MethodBase targetMethod = AccessTools.Method(typeof(Pawn), "WorkTypeIsDisabled");
                MethodBase mechCheckMethod = AccessTools.Method(typeof(Utils), "IsConsideredMechanical", new Type[] { typeof(Pawn) });
                CodeInstruction startInstruction = new CodeInstruction(OpCodes.Ldarg_1);
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                bool needNextLabel = false;
                int insertionPoint = -1;
                Label originalStartConditionLabel = generator.DefineLabel();
                Label? skipBranchLabel = new Label?();

                // Locate the troublesome doctor work type check and identify the location to insert our instructions at.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].operand as MethodBase == targetMethod)
                    {
                        // Mark the place where our instruction goes.
                        insertionPoint = i - 3;
                        // Move any jumps over to our start instruction so they don't get skipped.
                        instructions[i - 3].MoveLabelsTo(startInstruction);
                        // Store the original condition start instruction so that if the Mechanical check fails it doesn't get skipped entirely.
                        instructions[i - 3].labels.Add(originalStartConditionLabel);
                        needNextLabel = true;
                    }
                    // Identify wherever the condition was originally branching out to if false and store for later.
                    else if (needNextLabel && instructions[i].Branches(out skipBranchLabel))
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
                        // If (Utils.IsConsideredMechanical(pawn) && pawn.WorkTypeIsDisabled(ATR_WorkTypeDefOf.ATR_Mechanic))
                        yield return startInstruction; // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, mechCheckMethod); // Call Utils.IsConsideredMechanical(pawn)
                        yield return new CodeInstruction(OpCodes.Brfalse_S, originalStartConditionLabel); // Branch to original condition (or'd) if false to check against Doctor

                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load Pawn again
                        yield return new CodeInstruction(OpCodes.Ldsfld, typeof(ATR_WorkTypeDefOf).GetField("ATR_Mechanic")); // Load the ATR_Mechanic WorkTypeDef onto the stack
                        yield return new CodeInstruction(OpCodes.Call, targetMethod); // Call pawn.WorkTypeIsDisabled(ATR_Mechanic);
                        yield return new CodeInstruction(OpCodes.Brfalse_S, skipBranchLabel); // Is a mechanical pawn but can't do Mechanic, condition is true, no need to check original condition.

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