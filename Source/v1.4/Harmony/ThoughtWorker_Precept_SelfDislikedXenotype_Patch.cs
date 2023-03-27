using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class ThoughtWorker_Precept_SelfDislikedXenotype_Patch
    {
        // Mechanical units are unaffected by preferred xenotype social effects as they can not have genetics or xenotypes.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_SelfDislikedXenotype), "ShouldHaveThought")]
        public class ShouldHaveThought_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanical(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}