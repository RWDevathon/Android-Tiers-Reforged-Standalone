using Verse;
using HarmonyLib;
using RimWorld;
using System;

namespace ATReforged
{
    internal class PawnDiedOrDownedThoughtsUtility_Patch
    {
        // If the dead pawn is a drone or a surrogate, no one cares about them dying. They don't possess true intelligence.
        [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "TryGiveThoughts")]
        [HarmonyPatch(new Type[] {typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind)})]
        public class TryGiveThoughts_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(ref Pawn victim)
            { 
                return !(Utils.IsConsideredMechanicalDrone(victim) || Utils.IsSurrogate(victim));
            }
        }
    }
}