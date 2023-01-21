using System;
using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class JobGiver_GetRest_Patch
    {
        // Override job for getting rest based on whether the pawn can charge instead or not. Pawns that can rest and charge will seek to do both simultaneously.
        [HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
        public class TryGiveJob_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Job __result)
            {
                try
                {
                    // If the pawn can't use charging or isn't on a map, then there's nothing to override. 
                    if (pawn != null && pawn.Map != null && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer) && Utils.CanUseBattery(pawn))
                    {
                        // Don't override non-spawned or drafted pawns.
                        if (!pawn.Spawned || pawn.Drafted)
                            return;
                        
                        // Attempt to locate a viable charging bed for the pawn. This can suit comfort, rest, and room needs whereas the charging station can not.
                        Building_Bed bed = Utils.GetChargingBed(pawn, pawn);
                        if (bed != null)
                        {
                            pawn.ownership.ClaimBedIfNonMedical(bed);
                            __result = new Job(ATR_JobDefOf.ATR_RechargeBattery, new LocalTargetInfo(bed));
                            return;
                        }
                    }
                    // If there is no viable charging bed, then the pawn is free to use whatever bed it was originally planning to rest in.
                }
                catch (Exception ex)
                {
                    Log.Warning("[ATR] ATReforged.JobGiver_GetRest_Patch Encountered an error while attempting to check pawn" + pawn + " for charging. Default vanilla behavior will proceed." + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}