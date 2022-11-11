using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    // Deceased drones or surrogates don't get unburied notifications.
    internal class Alert_ColonistLeftUnburied_Patch
    {
        [HarmonyPatch(typeof(Alert_ColonistLeftUnburied), "IsCorpseOfColonist")]
        public class Alert_ColonistLeftUnburied_IsCorpseOfColonist_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Corpse corpse, ref bool __result)
            {
                if (!__result)
                    return;

                Pawn p = corpse.InnerPawn;
                if (p != null && (Utils.IsConsideredMechanicalDrone(p) || Utils.IsSurrogate(p)))
                    __result = false;
 
            }
        }
    }
}