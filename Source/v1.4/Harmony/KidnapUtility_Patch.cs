using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class KidnapUtility_Patch
    {
        // Ensure kidnapped pawns are disconnected from the SkyMind. This will handle surrogates, mind operations, and capacities.
        [HarmonyPatch(typeof(KidnapUtility), "IsKidnapped")]
        public class KidnappedPawnsDisconnectFromSkyMind_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref bool __result)
            {
                if (__result)
                {
                    // This will handle the whole process. It will do nothing if it wasn't connected to the SkyMind already.
                    Utils.gameComp.DisconnectFromSkyMind(pawn);
                }
            }
        }
    }
}