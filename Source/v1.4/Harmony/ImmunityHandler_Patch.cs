using Verse;
using HarmonyLib;
using System;

namespace ATReforged
{
    internal class ImmunityHandler_Patch
    {
        // Mechanical units are immune to some diseases by default. If they are not immune to it for some other reason, they are immune if it is a blacklisted disease.
        [HarmonyPatch(typeof(ImmunityHandler), "DiseaseContractChanceFactor")]
        [HarmonyPatch(new Type[] { typeof(HediffDef), typeof(BodyPartRecord) })]
        public class DiseaseContractChanceFactor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(HediffDef diseaseDef, BodyPartRecord part, ref float __result, Pawn ___pawn)
            {
                if (__result != 0f && Utils.IsConsideredMechanical(___pawn) && Utils.ReservedBlacklistedDiseases.Contains(diseaseDef.defName))
                {
                    __result = 0;
                }
            }
        }
    }
}