using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    internal class InteractionWorker_EnslaveAttempt_Patch
    {
        /* TODO: Verify this path is unnecessary as surrogates disconnect long before they can be imprisoned, let alone enslaved.
        // Surrogates disconnect on enslavement attempts.
        [HarmonyPatch(typeof(InteractionWorker_EnslaveAttempt), "Interacted")]
        public class InteractionWorker_EnslaveAttempt_Interacted
        {
            [HarmonyPostfix]
            public static void Listener(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, string letterText, string letterLabel, LetterDef letterDef, LookTargets lookTargets)
            {
                if (recipient.IsSlave && Utils.IsSurrogate(recipient) && recipient.TryGetComp<CompSkyMindLink>().isForeign)
                {
                    recipient.TryGetComp<CompSkyMindLink>().DisconnectController();
                }
            }
        }
        */
    }
}