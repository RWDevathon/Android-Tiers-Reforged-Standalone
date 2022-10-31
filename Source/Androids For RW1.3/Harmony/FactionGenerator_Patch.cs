using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace ATReforged
{
    internal class FactionGenerator_Patch

    {
        // When factions are generated, remove android factions if they are forbidden.
        [HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld")]
        public class Ingested_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Dictionary<FactionDef, int> factionCounts)
            {
                try
                {
                      Utils.GCATPP.CheckDeleteAndroidFactions();
                }
                catch(Exception e)
                {
                    Log.Message("[ATPP] FactionGenerator.GenerateFactionsIntoWorld : " + e.Message + " - " + e.StackTrace);
                }
            }
        }
    }
}