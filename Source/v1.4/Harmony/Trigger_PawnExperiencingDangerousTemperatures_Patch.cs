using Verse;
using HarmonyLib;
using System.Collections.Generic;
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

                    // The targetHediffs taken in the last step are all hediffs which are temperature related and need to be checked against the hediffs present on this pawn.
                    HashSet<HediffDef> targetHediffDefs = Utils.GetTemperatureHediffDefsForRace(pawn.RaceProps);
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (targetHediffDefs.Contains(hediff.def) && hediff.Severity > 0.15f)
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