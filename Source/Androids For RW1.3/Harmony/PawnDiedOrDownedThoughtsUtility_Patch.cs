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
    internal class PawnDiedOrDownedThoughtsUtility_Patch
    {
        [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
        public class AppendThoughts_ForHumanlike_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(ref Pawn victim)
            { // If the dead pawn is a drone or a surrogate, no one cares about them dying. They don't possess true intelligence.
                return !(Utils.IsConsideredMechanicalDrone(victim) || Utils.IsSurrogate(victim));
            }
        }
    }
}