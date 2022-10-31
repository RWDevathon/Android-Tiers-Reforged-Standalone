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
    internal class IncidentWorker_WandererJoin_Patch

    {
        /*
         * PreFix to prevent ManInBlack event if it's caused by mind upload techs (for example all pawns in mind duplicate resulting in all downed, and thus the manInBlack event is fired
         */
        [HarmonyPatch(typeof(IncidentWorker_WandererJoin), "TryExecuteWorker")]
        public class PotentialVictims_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(IncidentWorker_WandererJoin __instance, IncidentParms parms, ref bool __result)
            {
                try
                {
                    if (__instance.def.defName != "StrangerInBlackJoin")
                        return true;

                    Map map = (Map)parms.target;
                    foreach(var e in Find.ColonistBar.Entries)
                    {
                        Pawn cp = e.pawn;
                        //pawn downed but this is not permanent => no ManInBlack
                        if (cp != null && !cp.Dead && cp.health.hediffSet.HasHediff(HediffDefOf.ATR_MindOperation))
                        {
                            __result = false;
                            return false;
                        }
                    }

                    return true;
                }
                catch(Exception e)
                {
                    Log.Message("[ATPP] IncidentWorker_WandererJoin.TryExecuteWorker : " + e.Message + " " + e.StackTrace);
                    return true;
                }
            }
        }
    }
}