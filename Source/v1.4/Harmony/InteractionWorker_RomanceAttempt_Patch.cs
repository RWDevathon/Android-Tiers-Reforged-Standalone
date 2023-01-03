using Verse;
using RimWorld;
using HarmonyLib;

namespace ATReforged
{
    internal class InteractionWorker_RomanceAttempt_Patch
    {
        // Ensure surrogates are not picked for random romance attempts or attempt to randomly initiate them as they can not succeed.
        [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
        public class RandomSelectionWeight_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn initiator, Pawn recipient, ref float __result)
            {
                if (Utils.IsSurrogate(initiator) || Utils.IsSurrogate(recipient))
                    __result = 0f;
            }
        }

        // Romance attempts on surrogates will always fail to avoid relations with temporary consciousnesses.
        [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
        public class SuccessChance_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn initiator, Pawn recipient, ref float __result, float baseChance = 0.6f)
            {
                if (Utils.IsSurrogate(initiator) || Utils.IsSurrogate(recipient))
                    __result = 0;
            }
        }
    }
}