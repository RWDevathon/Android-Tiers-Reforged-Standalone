using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class Need_Patch
    {
        // Mechanicals don't have a food meter, they have an energy meter. Since we're hijacking hunger, change the labelled name for mechanicals.
        [HarmonyPatch(typeof(Need), "get_LabelCap")]
        public class get_LabelCap
        {
            [HarmonyPostfix]
            public static void Listener( ref string __result, Pawn ___pawn, Need __instance)
            {
                if (__instance.def.defName == "Food")
                {
                    if (Utils.CanUseBattery(___pawn))
                    {
                        __result = "ATR_EnergyNeed".Translate();
                    }
                }
            }
        }

        // Ensure the tooltip for hunger displays a tooltip about energy for mechanicals.
        [HarmonyPatch(typeof(Need_Food), "GetTipString")]
        public class GetTipString_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref string __result, Pawn ___pawn, Need __instance)
            {
                if (__instance.def.defName == "Food")
                {
                    if (Utils.CanUseBattery(___pawn))
                    {
                        __result = string.Concat(new string[]
                            {
                                "ATR_EnergyNeed".Translate(),
                                ": ",
                                __instance.CurLevelPercentage.ToStringPercent(),
                                " (",
                                __instance.CurLevel.ToString("0.##"),
                                " / ",
                                __instance.MaxLevel.ToString("0.##"),
                                ")\n",
                                "ATR_EnergyNeedDesc".Translate()
                            });
                    }
                }
            }
        }
    }
}