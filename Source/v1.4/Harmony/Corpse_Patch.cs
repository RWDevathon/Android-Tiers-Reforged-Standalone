using System;
using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace ATReforged
{
    internal class Corpse_Patch
    {
        // No one is bothered by seeing a destroyed mechanical chassis. It doesn't rot, decay, or deteriorate significantly. It may not even have had an intelligence when destroyed.
        [HarmonyPatch(typeof(Corpse), "GiveObservedThought")]
        public class GiveObservedThought_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Corpse __instance, ref Thought_Memory __result)
            {
                if (Utils.IsConsideredMechanical(__instance.InnerPawn))
                {
                    __result = null;
                }
            }
        }

        // No one is bothered by seeing a destroyed mechanical chassis. It doesn't rot, decay, or deteriorate significantly. It may not even have had an intelligence when destroyed.
        [HarmonyPatch(typeof(Corpse), "GiveObservedHistoryEvent")]
        public class GiveObservedHistoryEvent_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Corpse __instance, Pawn observer, ref HistoryEventDef __result)
            {
                if (Utils.IsConsideredMechanical(__instance.InnerPawn))
                {
                    __result = null;
                }
            }
        }

        // This transpiler ensures that butchering drones does not create a butcher thought or history event.
        [HarmonyPatch]
        public class ButcherProducts_Patch
        {
            [HarmonyPatch]
            static MethodInfo TargetMethod()
            {
                return typeof(Corpse).GetNestedTypes(AccessTools.all).First(ty => ty.Name.Contains("<ButcherProducts>")).GetMethods(AccessTools.all).First(m => PatchProcessor.GetOriginalInstructions(m).Any(inst => inst.Calls(AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike)))));
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                var type = typeof(Corpse).GetNestedTypes(AccessTools.all).First(ty => ty.Name.Contains("<ButcherProducts>"));
                var fieldInfo = type.GetFields(AccessTools.all).First(field => field.FieldType == typeof(Corpse));

                foreach (CodeInstruction inst in insts)
                {
                    if (inst.Calls(AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike))))
                    {

                        yield return inst;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, fieldInfo);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Corpse), nameof(Corpse.InnerPawn)));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.IsConsideredMechanicalDrone), new Type[] { typeof(Pawn) }));
                        yield return new CodeInstruction(OpCodes.Not);
                        yield return new CodeInstruction(OpCodes.And);

                        continue;
                    }
                    yield return inst;
                }
            }
        }
    }
}