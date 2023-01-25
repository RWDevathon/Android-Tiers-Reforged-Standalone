using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ATReforged
{
    // Assign Building Owner has an ideology check but does not null-check it if Ideology is installed. This transpiler adds an Ideo check to it to prevent drones breaking the dialogue interface.
    internal class Dialog_AssignBuildingOwner_Patch
    {
        [HarmonyPatch(typeof(Dialog_AssignBuildingOwner), "DoWindowContents")]
        public class DoWindowContents_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                MethodBase targetMethod = AccessTools.PropertyGetter(typeof(Find), "IdeoManager");
                MethodBase targetPawnMethod = AccessTools.Method(typeof(CompAssignableToPawn), "IdeoligionForbids");
                CodeInstruction startInstruction = new CodeInstruction(OpCodes.Ldloc_S, 25);
                CodeInstruction pawnInstruction = null;
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                bool needNextLabel = false;
                int insertionPoint = -1;
                Label? skipBranchLabel = null;

                // Locate the troublesome Ideology content check and identify its index for our insertion point.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].operand as MethodBase == targetMethod)
                    {
                        // Mark the place where our instruction goes.
                        insertionPoint = i;
                        // Move any jumps over to our start instruction so they don't get skipped.
                        instructions[i].MoveLabelsTo(startInstruction);
                        needNextLabel = true;
                    }
                    // Identify and save instructions that will load the local pawn data
                    else if (instructions[i].operand as MethodBase == targetPawnMethod)
                    {
                        pawnInstruction = instructions[i - 1];
                    }
                    // Identify wherever the condition was originally branching out to if false and store for later.
                    else if (needNextLabel && instructions[i].Branches(out skipBranchLabel))
                    {
                        needNextLabel = false;
                    }

                    // If we have acquired both of the details we need, there is no reason to continue iterating.
                    if (pawnInstruction != null && skipBranchLabel != null)
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
                        // If (pawn.ideo != null)
                        yield return startInstruction; // Load local function memory (foreach loop)
                        yield return pawnInstruction; // Load the Pawn stored in the local scope
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Pawn).GetField("ideo")); // Get Pawn's IdeoTracker
                        yield return new CodeInstruction(OpCodes.Brfalse_S, skipBranchLabel); // If null, branch out of the problematic area.

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