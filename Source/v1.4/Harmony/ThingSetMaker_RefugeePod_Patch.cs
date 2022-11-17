using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    internal class ThingSetMaker_RefugeePod_Patch
    {
        // Mechanicals coming in refugee pods will likely not be downed appropriately. Apply the long reset hediff to them so that they may be properly rescued or captured safely.
        [HarmonyPatch(typeof(ThingSetMaker_RefugeePod), "Generate")]
        public class Generate_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ThingSetMakerParams parms, List<Thing> outThings)
            {
                foreach (Thing thing in outThings)
                {
                    if (thing is Pawn pawn && Utils.IsConsideredMechanical(pawn))
                    {
                        pawn.health.AddHediff(HediffDefOf.ATR_LongReboot);
                    }
                }
            }
        }
    }
}