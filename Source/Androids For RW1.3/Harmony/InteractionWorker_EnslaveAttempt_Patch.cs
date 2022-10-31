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
    internal class InteractionWorker_EnslaveAttempt_Patch
    {
        [HarmonyPatch(typeof(InteractionWorker_EnslaveAttempt), "Interacted")]
        public class InteractionWorker_EnslaveAttempt_Interacted
        {
            // Patch to ensure surrogate owners disconnect if their surrogate is enslaved. Given the emergency disconnect, dead man's hand is not triggered.
            [HarmonyPostfix]
            public static void Listener(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, string letterText, string letterLabel, LetterDef letterDef, LookTargets lookTargets)
            {
                if (recipient.IsSlave && Utils.IsSurrogate(recipient) && recipient.TryGetComp<CompSkyMindLink>().isForeign)
                {
                    recipient.TryGetComp<CompSkyMindLink>().DisconnectController();
                }
            }
        }
    }
}