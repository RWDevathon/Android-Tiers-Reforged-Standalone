using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class ThoughtWorker_Precept_HasProsthetic_Patch
    {
        // Mechanical units are a prosthetic. So they have one.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_HasProsthetic), "HasProsthetic")]
        public class TW_Precept_HasProsthetic_HasProsthetic
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref bool __result)
            {
                if (__result)
                    return;

                if (Utils.IsConsideredMechanicalAndroid(p))
                {
                    __result = true;
                }
            }
        }
    }
}