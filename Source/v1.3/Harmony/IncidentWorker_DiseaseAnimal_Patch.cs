using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace ATReforged
{
    internal class IncidentWorker_DiseaseAnimal_Patch
    {
        // Remove all mechanical animals from the candidate list for animal diseases.
        [HarmonyPatch(typeof(IncidentWorker_DiseaseAnimal), "PotentialVictimCandidates")]
        public class PotentialVictims_Patch
        {
            [HarmonyPostfix]
            public static void Listener(IIncidentTarget target, ref IEnumerable<Pawn> __result)
            {
                if (__result == null)
                    return;

                __result = __result.Where(pawn => !Utils.IsConsideredMechanicalAnimal(pawn));
            }
        }
    }
}