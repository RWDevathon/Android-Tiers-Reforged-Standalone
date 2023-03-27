using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;

namespace ATReforged
{
    internal class ThoughtWorker_Precept_ColonyXenotypeMakeup_Patch
    {
        // Mechanical units are unaffected by preferred xenotype social effects as they can not have genetics or xenotypes.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_ColonyXenotypeMakeup), "ShouldHaveThought")]
        public class ShouldHaveThought_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                bool insertAfter = false;
                Label? branchLabel = null;

                // Locate the IsPrisoner check and add the mechanical check after it.
                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.IsPrisoner))))
                    {
                        insertAfter = true;
                    }
                    else if (insertAfter && instructions[i].Branches(out branchLabel))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.IsConsideredMechanical), new Type[] { typeof(Pawn) })); // Our function call
                        yield return new CodeInstruction(OpCodes.Brtrue_S, branchLabel); // Branch to the code beyond the conditional if true, we don't count mechanical pawns.
                        insertAfter = false;
                    }
                }
            }
        }
    }
}