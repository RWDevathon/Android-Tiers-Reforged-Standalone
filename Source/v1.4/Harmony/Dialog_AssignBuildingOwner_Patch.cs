namespace ATReforged
{
    // Assign Building Owner has an ideology check but does not null-check it if Ideology is installed. This transpiler adds an Ideo check to it to prevent drones breaking the dialogue interface.
    /* TODO: Make this transpiler and ensure it works.
    internal class Dialog_AssignBuildingOwner_Patch
    {
        [HarmonyPatch(typeof(Dialog_AssignBuildingOwner), "DoWindowContents")]
        public class DoWindowContents_Patch
        {

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                MethodBase targetMethod = AccessTools.Method(typeof(Find), "IdeoManager");
                MethodBase ideoCheckMethod = AccessTools.Method(typeof(Pawn), "Ideo");
                CodeInstruction startInstruction = new CodeInstruction(OpCodes.Ldloc_S, 25);
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                bool needNextLabel = false;
                int insertionPoint = -1;
                Label? skipBranchLabel = new Label?();

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
                        yield return startInstruction; // Load Dialogue_AssignBuildingOwner
                        yield return new CodeInstruction(OpCodes.Ldfld, mechCheckMethod); // Call Utils.IsConsideredMechanical(pawn)
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
    */
}