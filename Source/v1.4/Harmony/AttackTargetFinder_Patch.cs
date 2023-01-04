using System;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace ATReforged
{
    internal class AttackTargetFinder_Patch
    {
        // This transpiler alters a validator for AttackTargetFinder to ensure that enemy AI's with EMP weapons will consider androids as vulnerable targets for attacking.
        // Otherwise, enemies with EMP weapons will ignore androids entirely, preferring to melee or simply outright ignoring.
        [HarmonyPatch]
        public class BestAttackTarget_innerValidator_Patch
        {
            [HarmonyPatch]
            static MethodInfo TargetMethod()
            {
                return typeof(AttackTargetFinder).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(target => target.ReturnType == typeof(bool) && target.GetParameters().First().ParameterType == typeof(IAttackTarget));
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                bool needNextLabel = false;
                int insertionPoint = -1;
                Label? insertLabelEnd = new Label?();

                // Locate the IsFlesh check for the EMP vulnerability piece and mark it as our insertion point.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].Calls(AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsFlesh))))
                    {
                        // Mark the place where our instruction goes.
                        insertionPoint = i + 1;
                        needNextLabel = true;
                    }
                    else if (needNextLabel && instructions[i].Branches(out insertLabelEnd))
                    {
                        // Branches will insert the label directly into the variable for us. Break to avoid looking for other insertion points unnecessarily.
                        break;
                    }
                }

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    // Operation target hit, yield contained instructions and add null-check branch.
                    if (insertionPoint == i)
                    {
                        yield return instructions[i]; // Return the instruction we encountered initially
                        yield return new CodeInstruction(OpCodes.Ldloc_1); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.IsConsideredMechanical), new Type[] { typeof(Pawn) })); // Our function call
                        yield return new CodeInstruction(OpCodes.Brtrue_S, insertLabelEnd); // Branch to next check if it is a mechanical unit (if Utils.IsConsideredMechanical == true)
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