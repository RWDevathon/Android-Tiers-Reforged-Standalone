using Verse;
using HarmonyLib;
using RimWorld.Planet;
using RimWorld;

namespace ATReforged
{
    // Non-player pawns being passed to the world can never be connected to the SkyMind network (they may have just been kidnapped, sold, or lost).
    [HarmonyPatch(typeof(WorldPawns), "PassToWorld")]
    public class PassToWorld_Patch
    {
        [HarmonyPostfix]
        public static void Listener(Pawn pawn, PawnDiscardDecideMode discardMode)
        {
            if (pawn.Faction != Faction.OfPlayerSilentFail)
            {
                // This is an always safe operation, only pawns that are stored will actually be removed.
                Utils.gameComp.DisconnectFromSkyMind(pawn);
            }
        }
    }
}