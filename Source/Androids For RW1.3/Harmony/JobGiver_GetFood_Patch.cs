using System;
using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Linq;

namespace ATReforged
{
    internal class JobGiver_GetFood_Patch
    {
        // Override job for getting food based on whether the pawn can charge instead or not.
        [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
        public class TryGiveJob_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Job __result)
            {
                try
                {
                    // If the pawn can't use charging or isn't on a map, then there's nothing to override. 
                    if (pawn != null && pawn.Map != null && Utils.CanUseBattery(pawn))
                    {
                        // Don't override non-spawned or drafted pawns.
                        if (!pawn.Spawned || pawn.Drafted)
                            return;
                        
                        // Attempt to locate a viable charging bed for the pawn. This can suit comfort, rest, and room needs whereas the charging station can not.
                        Building_Bed bed = Utils.GetAvailableChargingBed(pawn);
                        if (bed != null)
                        {
                            __result = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(bed));
                            return;
                        }

                        // Attempt to locate a viable charging station. Set the result to this if one is found.
                        foreach (Building station in pawn.Map.listerBuildings.allBuildingsColonist.Where(building => building.TryGetComp<CompChargingStation>() != null).OrderBy(building => building.Position.DistanceToSquared(pawn.Position)))
                        {
                            if (!station.Destroyed && !station.IsBrokenDown() && (bool)station.TryGetComp<CompPowerTrader>()?.PowerOn && station.Position.InAllowedArea(pawn))
                            {
                                IntVec3 freePlace = station.TryGetComp<CompChargingStation>().GetOpenRechargeSpot(pawn);
                                if (freePlace != IntVec3.Invalid && pawn.CanReach(freePlace, PathEndMode.OnCell, Danger.Deadly))
                                {
                                    __result = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(station.TryGetComp<CompChargingStation>().GetOpenRechargeSpot(pawn)), new LocalTargetInfo(station));
                                    return;
                                }
                            }
                        }
                    }
                    // If there is no viable charging bed or charging station, then the pawn is free to grab whatever food it was originally planning to consume.
                }
                catch (Exception ex)
                {
                    Log.Warning("[ATR] ATReforged.JobGiver_GetFood_Patch Encountered an error while attempting to check pawn" + pawn + " for charging. Default vanilla behavior will proceed." + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}