using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class JobGiver_PatientGoToBed_Patch
    {
        // Override job for being a patient based on whether the pawn can charge and the target bed is charge-capable.
        [HarmonyPatch(typeof(JobGiver_PatientGoToBed), "TryIssueJobPackage")]
        public class TryIssueJobPackage_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, JobIssueParams jobParams, JobGiver_PatientGoToBed __instance, ref ThinkResult __result)
            {
                if (__result == ThinkResult.NoJob || __result.Job.targetA.Thing.TryGetComp<CompPowerTrader>() == null || !Utils.CanUseBattery(pawn))
                    return;

                __result = new ThinkResult(JobMaker.MakeJob(JobDefOf.RechargeBattery, __result.Job.targetA.Thing), __instance);
            }
        }
    }
}