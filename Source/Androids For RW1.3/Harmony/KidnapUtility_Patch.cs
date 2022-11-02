using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ATReforged
{
    internal class KidnapUtility_Patch
    {
        // Ensure kidnapped pawns are disconnected from the SkyMind. This will handle surrogates, mind operations, and capacities.
        [HarmonyPatch(typeof(KidnapUtility), "IsKidnapped")]
        public class KidnappedPawnsDisconnectFromSkyMind_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn)
            {
                // This will handle the whole process. It will do nothing if it wasn't connected to the SkyMind already.
                Utils.GCATPP.DisconnectFromSkyMind(pawn);

                // If this unit was providing SkyMind capacity, we need to remove it so they don't provide capacity while kidnapped.
                if (pawn.TryGetComp<CompSkyMindCore>() != null)
                {
                    Utils.GCATPP.RemoveCore(pawn.TryGetComp<CompSkyMindCore>());
                }
            }
        }
    }
}