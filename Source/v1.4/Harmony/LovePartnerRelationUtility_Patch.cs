using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    public class LovePartnerRelationUtility_Patch
    {
        // Pawns will consider surrogates of their loved one to also be a loved one.
        [HarmonyPatch(typeof(LovePartnerRelationUtility), "LovePartnerRelationExists")]
        public class RandomSelectionWeight_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn first, Pawn second, ref bool __result)
            {
                if (__result)
                {
                    return;
                }

                // Check the first pawn for surrogate status.
                if (Utils.IsSurrogate(first) && LovePartnerRelationUtility.LovePartnerRelationExists(first.GetComp<CompSkyMindLink>().GetSurrogates().FirstOrFallback(), second))
                {
                    __result = true;
                }
                else if (Utils.IsSurrogate(second) && LovePartnerRelationUtility.LovePartnerRelationExists(first, second.GetComp<CompSkyMindLink>().GetSurrogates().FirstOrFallback()))
                {
                    __result = true;
                }
            }
        }
    }
}