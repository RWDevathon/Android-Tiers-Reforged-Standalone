using RimWorld;
using HarmonyLib;
using Verse;

namespace ATReforged
{
    internal static class ITab_Pawn_Visitor_Patch
    {
        // Drones are incapable of receiving social interactions, and are thus invalid for all prisoner interactions. Prevent players from interacting with the menu.
        [HarmonyPatch(typeof(ITab_Pawn_Visitor), "FillTab")]
        public class FillTab_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ITab_Pawn_Visitor __instance)
            {
                // Get the currently selected pawn. If it's null for some reason, let the default behavior continue.
                Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
                if (pawn == null)
                    return true;

                // Non-drones or non-prisoners/slaves should continue with default behavior.
                if (!Utils.IsConsideredMechanicalDrone(pawn) || !(pawn.IsPrisoner || pawn.IsSlave))
                    return true;

                // Send a message occasionally about the illegality of drones being prisoners.
                if (pawn.IsHashIntervalTick(30))
                {
                    if (pawn.IsPrisoner)
                    {
                        Messages.Message("ATR_PrisonerDroneWarning".Translate(), MessageTypeDefOf.NeutralEvent, false);
                    }
                    else if (pawn.IsSlave)
                    {
                        Messages.Message("ATR_SlaveDroneAlert".Translate(), MessageTypeDefOf.NegativeEvent, true);
                    }
                }
                // Prevent displaying the prisoner tab. There is nothing necessary on it for drones.
                return false;
            }
        }
    }
}