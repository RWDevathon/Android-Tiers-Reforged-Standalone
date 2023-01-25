using Verse;
using HarmonyLib;
using RimWorld;
using System;

namespace ATReforged
{
    // Drones can not participate in rituals.
    internal class RitualRoleAssignments_Patch
    {
        // Drones can not engage in rituals as spectators.
        [HarmonyPatch(typeof(RitualRoleAssignments), "PawnNotAssignableReason")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(RitualRole), typeof(Precept_Ritual), typeof(RitualRoleAssignments), typeof(TargetInfo), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]

        public class PawnNotAssignableReason_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, RitualRole role, ref string __result)
            {
                // If this pawn is invalid for some other reason, allow that to take priority.
                if (__result != null)
                    return;

                // Drones are not allowed to hold any roles in rituals.
                if (Utils.IsConsideredMechanicalDrone(p))
                {
                    __result = "ATR_DronesCannotParticipateInRituals".Translate();
                }
            }
        }
    }
}