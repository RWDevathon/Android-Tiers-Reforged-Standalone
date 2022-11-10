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
    internal class JobGiver_GetFood_Patch
    {
        // Override job for getting food based on whether the pawn can charge instead or not.
        [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
        public class TryGiveJob_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Job __result)
            {
                // If the pawn can't use charging, then there's nothing to override. 
                if (pawn != null && Utils.CanUseBattery(pawn))
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
                    Building chargingStation = Utils.gameComp.GetClosestFreeChargingStation(pawn.Map, pawn);
                    if (chargingStation != null)
                    {
                        __result = new Job(JobDefOf.RechargeBattery, new LocalTargetInfo(chargingStation.TryGetComp<CompChargingStation>().GetOpenRechargeSpot(pawn)), new LocalTargetInfo(chargingStation));
                    }
                }
                // If there is no viable charging bed or charging station, then the pawn is free to grab whatever food it was originally planning to consume.
            }
        }
    }
}