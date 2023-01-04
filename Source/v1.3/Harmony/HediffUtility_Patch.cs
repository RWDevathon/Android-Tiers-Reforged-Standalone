using Verse;
using HarmonyLib;

namespace ATReforged
{
    internal class HediffUtility_Patch
    {
        // Mechanical units get a large number of bonus "implanted" parts when checked for number so transhumanists love being mechanical.
        [HarmonyPatch(typeof(HediffUtility), "CountAddedAndImplantedParts")]
        public class CountAddedParts_Patch
        {
            [HarmonyPostfix]
            public static void Listener(HediffSet hs, ref int __result)
            {
                if (Utils.IsConsideredMechanical(hs.pawn))
                {
                    __result += 20;
                }
            }
        }
    }
}