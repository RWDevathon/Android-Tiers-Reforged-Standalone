using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class ThoughtWorker_NeedNeuralSupercharge_Patch

    {
        // Mechanical units do not have neural networks that can be supercharged like organics.
        [HarmonyPatch(typeof(ThoughtWorker_NeedNeuralSupercharge), "ShouldHaveThought")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanicalAndroid(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}