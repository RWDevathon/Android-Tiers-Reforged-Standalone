﻿using System;
using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;

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
                    if (pawn != null && pawn.Map != null && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer) && Utils.CanUseBattery(pawn))
                    {
                        // Don't override non-spawned or drafted pawns.
                        if (!pawn.Spawned || pawn.Drafted)
                            return;
                        
                        // Attempt to locate a viable charging bed for the pawn. This can suit comfort, rest, and room needs whereas the charging station can not.
                        Building_Bed bed = RestUtility.FindBedFor(pawn);
                        if (bed != null)
                        {
                            pawn.ownership.ClaimBedIfNonMedical(bed);
                            __result = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(bed));
                            return;
                        }

                        // Attempt to locate a viable charging station. Set the result to this if one is found.
                        Building station = (Building)GenClosest.ClosestThingReachable(pawn.PositionHeld, pawn.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn), validator: building => building.TryGetComp<CompPowerTrader>() != null && building.TryGetComp<CompPowerTrader>().PowerOn && building.TryGetComp<CompChargingStation>()?.GetOpenRechargeSpot(pawn) != null);
                        if (station != null)
                        {
                            __result = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(station.TryGetComp<CompChargingStation>().GetOpenRechargeSpot(pawn)), new LocalTargetInfo(station));
                            return;
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