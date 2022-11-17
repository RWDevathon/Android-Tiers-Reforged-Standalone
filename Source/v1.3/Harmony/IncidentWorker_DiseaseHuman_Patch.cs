using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace ATReforged
{
    internal class IncidentWorker_DiseaseHuman_Patch
    {
        // Mechanicals aren't valid candidates for diseases.
        [HarmonyPatch(typeof(IncidentWorker_DiseaseHuman), "PotentialVictimCandidates")]
        public class PotentialVictimCandidates_Patch
        {
            [HarmonyPostfix]
            public static void Listener(IIncidentTarget target, ref IEnumerable<Pawn> __result)
            {
                if (__result == null)
                    return;

                __result = __result.Where(pawn => !Utils.IsConsideredMechanical(pawn));
            }
        }
    }
}