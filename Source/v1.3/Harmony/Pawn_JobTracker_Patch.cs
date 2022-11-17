using Verse;
using HarmonyLib;
using Verse.AI;
using RimWorld;

namespace ATReforged
{
    // Charge-capable pawns that are tucked in (via capture, rescue, for operations, etc) to a charge-capable bed will charge instead of resting normally.
    [HarmonyPatch(typeof(Pawn_JobTracker), "Notify_TuckedIntoBed")]
    internal static class Notify_TuckedIntoBed_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_JobTracker __instance, Pawn ___pawn, Building_Bed bed)
        {
            if (Utils.CanUseBattery(___pawn) && bed.PowerComp != null)
            {
                ___pawn.Position = RestUtility.GetBedSleepingSlotPosFor(___pawn, bed);
                ___pawn.Notify_Teleported(endCurrentJob: false);
                ___pawn.stances.CancelBusyStanceHard();
                __instance.StartJob(JobMaker.MakeJob(JobDefOf.RechargeBattery, bed), JobCondition.InterruptForced, tag: JobTag.TuckedIntoBed);
                return false;
            }
            return true;
        }
    }
}