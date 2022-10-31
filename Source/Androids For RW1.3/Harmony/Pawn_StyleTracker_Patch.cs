using Verse;
using HarmonyLib;
using RimWorld;
using System;

namespace ATReforged
{
    internal class Pawn_StyleTracker_Patch
    {
        /*
         * Prevent androids && surrogates from wanting to change their look
         */
        [HarmonyPatch(typeof(Pawn_StyleTracker), "RequestLookChange")]
        public class RequestLookChange_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn ___pawn)
            {
                if (Utils.IsConsideredMechanical(___pawn) || Utils.IsSurrogate(___pawn))
                {
                    return false;
                }
                return true;
            }
        }
    }
}