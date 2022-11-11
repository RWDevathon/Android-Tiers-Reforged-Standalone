using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    internal class ThoughtWorker_Precept_IdeoDiversity_Uniform_Patch
    {
        // Mechanical drones don't care about ideological diversity. Other pawns don't care about the drones' lack of ideology.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity_Uniform), "ShouldHaveThought")]
        public class TW_Precept_IdeoUniform_ShouldHaveThought
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p, ref ThoughtState __result)
            {
                if (p.Faction == null || !p.IsColonist || Utils.IsConsideredMechanicalDrone(p))
                {
                    __result = false;
                    return false;
                }
                List<Pawn> list = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
                int num = 0;
                foreach (Pawn pawn in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
                {
                    if (!pawn.IsQuestLodger() && pawn.RaceProps.Humanlike && !pawn.IsSlave && !pawn.IsPrisoner && !Utils.IsConsideredMechanicalDrone(pawn))
                    {
                        if (pawn.Ideo != p.Ideo)
                        {
                            __result = false;
                            return false;
                        }
                        num++;
                    }
                }

                __result = num > 0;
                return false;
            }
        }
    }
}