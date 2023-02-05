using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace ATReforged
{
    internal class RestUtility_Patch
    {
        // If the bed has a CompRestrictable on it and the assigned pawn type does not match the pawn's type, then it is not a valid bed for this pawn.
        [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
        public class IsValidBedFor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Thing bedThing, Pawn sleeper, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                PawnType assignedType = bedThing.TryGetComp<CompPawnTypeRestrictable>().assignedToType;
                if ((Utils.GetPawnType(sleeper) | assignedType) != assignedType)
                {
                    __result = false;
                }
            }
        }

        // RestUtility.TimetablePreventsLayDown has a non-null checked rest need that will throw errors for mechanical units resting when assigned to work in the time table.
        [HarmonyPatch(typeof(RestUtility))]
        [HarmonyPatch("TimetablePreventsLayDown")]
        public class TimetablePreventsLayDown_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                CodeInstruction startInstruction = new CodeInstruction(OpCodes.Ldarg_0);
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                List<CodeInstruction> insertInstructions = new List<CodeInstruction>();
                bool needNextLabel = false;
                int insertionPoint = -1;
                Label? insertLabelEnd = new Label?();

                // Locate the rest check that we need to add a null check for, and handle the appropriate features.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].Calls(AccessTools.PropertyGetter(typeof(Need), nameof(Need.CurLevel))))
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
                    // Branches gives us the label we need, so we can break out of this search when we possess it.
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
                        // If (pawn.Needs.Rest != null)
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