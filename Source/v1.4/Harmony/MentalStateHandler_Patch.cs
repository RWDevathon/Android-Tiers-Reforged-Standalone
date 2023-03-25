using Verse;
using HarmonyLib;
using RimWorld;
using Verse.AI;
using System.Collections;
using System.Collections.Generic;

namespace ATReforged
{
    // Surrogates that have mental states triggered on them will trigger it on their controller. Controllers will disconnect from their surrogates.
    internal class MentalStateHandler_Patch
    {
        [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
        public class TryStartMentalState_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Pawn ___pawn, MentalStateDef stateDef, string reason, bool forceWake, bool causedByMood, Pawn otherPawn, bool transitionSilently, bool causedByDamage, bool causedByPsycast)
            {
                // No need to continue if the mental state was not started for some reason.
                if (!__result)
                {
                    return;
                }

                // Not all pawns are SkyMindLinkable. Skip this method if that is the case.
                CompSkyMindLink compSkyMindLink = ___pawn.GetComp<CompSkyMindLink>();
                if (compSkyMindLink == null)
                {
                    return;
                }

                // This will return true if a surrogate or a controller. No special behavior is necessary if it is false.
                if (!compSkyMindLink.HasSurrogate())
                {
                    return;
                }

                // Pawns that have a surrogate connection are either a controller or a surrogate themselves. Handle cases separately.
                if (Utils.IsSurrogate(___pawn))
                {
                    // If the controller is in the SkyMind Core, do nothing.
                    Pawn controller = compSkyMindLink.GetSurrogates().FirstOrFallback();

                    // Less than extreme mental states simply apply a mood debuff to their controller and reboots this particular surrogate.
                    if (!stateDef.IsExtreme)
                    {
                        controller.needs.mood?.thoughts?.memories?.TryGainMemoryFast(ATR_ThoughtDefOf.ATR_SurrogateMentalBreak);
                        ___pawn.health.AddHediff(ATR_HediffDefOf.ATR_LongReboot);
                        Find.LetterStack.ReceiveLetter("ATR_SurrogateSufferedMentalState".Translate(), "ATR_SurrogateSufferedMentalStateDesc".Translate(), LetterDefOf.NegativeEvent);

                    }
                    // Extreme mental states are applied to the controller directly.
                    else
                    {
                        // If the controller is not a SkyMind Core intelligence, it will have the mental state applied directly.
                        if (!Utils.gameComp.GetCloudPawns().Contains(controller))
                        {
                            controller.mindState.mentalStateHandler.TryStartMentalState(stateDef, reason, forceWake, causedByMood, otherPawn, transitionSilently, causedByDamage, causedByPsycast);
                        }
                        // Surrogates of a SkyMind Core intelligence simply reboot upon suffering a mental break, regardless of extremity.
                        else
                        {
                            ___pawn.health.AddHediff(ATR_HediffDefOf.ATR_LongReboot);
                        }
                    }
                }
                // Controllers reboot all surrogates and disconnect them when suffering a mental break.
                else
                {
                    IEnumerable<Pawn> surrogates = compSkyMindLink.GetSurrogates();
                    foreach (Pawn surrogate in surrogates)
                    {
                        surrogate.health.AddHediff(ATR_HediffDefOf.ATR_LongReboot);
                    }
                    compSkyMindLink.DisconnectSurrogates();
                    Find.LetterStack.ReceiveLetter("ATR_ControllerSufferedMentalState".Translate(), "ATR_ControllerSufferedMentalStateDesc".Translate(), LetterDefOf.NegativeEvent);
                }
            }
        }
    }
}