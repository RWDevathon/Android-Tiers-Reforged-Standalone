using Verse;
using HarmonyLib;
using RimWorld;
using System.Linq;

namespace ATReforged
{
    internal class Pawn_InteractionsTracker_Patch
    {
        // Surrogate controllers/receivers don't interact with themselves. This could lead to weird things like marrying a surrogate/self.
        [HarmonyPatch(typeof(Pawn_InteractionsTracker), "CanInteractNowWith")]
        public class CanInteractNowWith_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn ___pawn, Pawn recipient, ref bool __result, InteractionDef interactionDef = null)
            {
                if (!__result)
                    return;

                // GetSurrogates returns all surrogates of a controller if the pawn is a controller, or the surrogate's controller if it is a surrogate.
                CompSkyMindLink link = ___pawn.GetComp<CompSkyMindLink>();
                if (link != null && link.HasSurrogate() && link.GetSurrogates().Contains(recipient))
                {
                    __result = false;
                }
            }
        }
    }
}