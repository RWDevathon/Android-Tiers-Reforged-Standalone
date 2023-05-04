using Verse;
using HarmonyLib;
using RimWorld.Planet;

namespace ATReforged
{
    public class WorldPawns_Patch
    {
        // Pawns being passed to the world can never be connected to the SkyMind network (they're no longer player pawns).
        [HarmonyPatch(typeof(WorldPawns), "PassToWorld")]
        public class PassToWorld_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, PawnDiscardDecideMode discardMode)
            {
                // This is an always safe operation, only pawns that are stored will actually be removed.
                Utils.gameComp.DisconnectFromSkyMind(pawn);
            }
        }
    }
}