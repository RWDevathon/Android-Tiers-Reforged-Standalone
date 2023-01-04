using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace ATReforged
{
    internal class ThoughtWorker_Precept_IdeoDiversity_Patch
    {
        // Mechanical drones don't care about ideological diversity. Other pawn's don't care about the drones' lack of ideology.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity), "ShouldHaveThought")]
        public class TW_Precept_IdeoDiversity_ShouldHaveThought
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p, ref ThoughtState __result, ThoughtWorker_Precept_IdeoDiversity __instance)
            {
                if (p.Faction == null || !p.IsColonist || Utils.IsConsideredMechanicalDrone(p))
                {
                    __result = false;
                    return false;
                }
                int num = 0;
                int num2 = 0;
                foreach (Pawn pawn in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
                {
                    if (!pawn.IsQuestLodger() && pawn.RaceProps.Humanlike && !pawn.IsSlave && !pawn.IsPrisoner && !Utils.IsConsideredMechanicalDrone(pawn))
                    {
                        num2++;
                        if (pawn != p && pawn.Ideo != p.Ideo)
                            num++;
                    }
                }
                if (num == 0)
                {
                    __result = ThoughtState.Inactive;
                    return false;
                }
                __result = ThoughtState.ActiveAtStage(Mathf.RoundToInt((float)num / (float)(num2 - 1) * (float)(__instance.def.stages.Count - 1)));
                return false;
            }
        }
    }
}