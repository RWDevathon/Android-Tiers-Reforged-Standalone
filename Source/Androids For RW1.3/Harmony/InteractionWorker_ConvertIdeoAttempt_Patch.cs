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
    internal class InteractionWorker_ConvertIdeoAttempt_Patch
    {
        // Mechanical Drones are not valid targets for ideological conversion.
        [HarmonyPatch(typeof(InteractionWorker_ConvertIdeoAttempt), "Interacted")]
        public class InteractionWorker_ConvertIdeoAttempt_Interactede_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
            {

                letterLabel = null;
                letterText = null;
                letterDef = null;
                lookTargets = null;

                if (Utils.IsConsideredMechanicalDrone(recipient))
                {
                    return false;
                }
                return true;
            }
        }
    }
}