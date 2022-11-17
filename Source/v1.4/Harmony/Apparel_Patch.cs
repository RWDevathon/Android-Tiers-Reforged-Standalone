using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class Apparel_Patch
    {
        // Dead mechanicals don't taint their clothes.
        [HarmonyPatch(typeof(Apparel), "Notify_PawnKilled")]
        public class Notify_PawnKilled_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Apparel __instance, ref bool ___wornByCorpseInt)
            {
                Pawn p = __instance.Wearer;

                if (p != null && Utils.IsConsideredMechanical(p))
                {
                    ___wornByCorpseInt = false;
                }
            }
        }
    }
}