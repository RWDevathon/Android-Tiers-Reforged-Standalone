using Verse;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse.AI.Group;

namespace ATReforged
{
    internal class Trigger_PawnExperiencingDangerousTemperatures_Patch
    {
        // Mechanical units check against different hediffs than organics do for temperature hediff concerns. We need to ensure they notify their lord group leader if any pawn is endangered with their unique hediffs.
        [HarmonyPatch(typeof(Trigger_PawnExperiencingDangerousTemperatures), "ActivateOn")]
        public class ActivateOn_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Lord lord, TriggerSignal signal)
            {
                // Check all pawns belonging to the lord.
                for (int i = lord.ownedPawns.Count - 1; i >= 0; i--)
                {
                    Pawn pawn = lord.ownedPawns[i];
                    // Skip pawns who can not or should not report danger to their lord.
                    if (!pawn.Spawned || pawn.Dead || pawn.Downed)
                    {
                        continue;
                    }

                    // Identify all hediffs that have a HediffGiver_Heat or HediffGiver_Hypothermia as their class - these are temperature hediffs.
                    List<HediffGiverSetDef> hediffGiverSetDefs = pawn.RaceProps.hediffGiverSets;
                    List<HediffDef> targetHediffs = new List<HediffDef>();
                    foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                    {
                        foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                        {
                            if (hediffGiver.GetType() == typeof(HediffGiver_Heat) || hediffGiver.GetType() == typeof(HediffGiver_Hypothermia))
                            {
                                targetHediffs.Add(hediffGiver.hediff);
                            }
                        }
                    }

                    // The targetHediffs taken in the last step are all hediffs which are temperature related and need to be checked against the hediffs present on this pawn.
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (targetHediffs.Contains(hediff.def) && hediff.Severity > 0.15f)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = false;
                return false;
            }
        }
    }
}