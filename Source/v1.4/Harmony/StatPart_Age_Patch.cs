using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class StatPart_Age_Patch
    {
        // Age as a StatPart is not used for mechanical units. This resolves issues around "baby" mechanical units with reduced work speed, and other related issues.
        [HarmonyPatch(typeof(StatPart_Age), "ActiveFor")]
        public class ActiveFor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref bool __result)
            {
                if (!__result)
                    return;

                if (pawn.ageTracker.CurLifeStage.defName == "MechanoidFullyFormed")
                {
                    __result = false;
                    return;
                }
            }
        }
    }
}