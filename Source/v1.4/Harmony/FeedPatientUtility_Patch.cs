using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    // There is no reason to try and feed a mechanical unit in a charging bed (something that doctors may otherwise try to do).
    internal class FeedPatientUtility_Patch
    {
        [HarmonyPatch(typeof(FeedPatientUtility), "ShouldBeFed")]
        public class ShouldBeFed_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref bool __result)
            {
                if (!__result || !Utils.IsConsideredMechanical(p))
                    return;


                Building_Bed building_Bed = p.CurrentBed();
                // Charging bed check
                if (building_Bed != null && building_Bed is Building_ChargingBed && building_Bed.TryGetComp<CompPowerTrader>()?.PowerOn == true)
                {
                    __result = false;
                    return;
                }

                // Bed with bedside charger check
                if (building_Bed != null && building_Bed.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading.Any(thing => thing.TryGetComp<CompPawnCharger>() != null && thing.TryGetComp<CompPowerTrader>()?.PowerOn == true) != null)
                {
                    __result = false;
                    return;
                }

                // Charging station check
                if (p.CurJob.def == JobDefOf.RechargeBattery && p.CurJob.targetA.Thing is Building_ChargingStation)
                    __result = false;
            }
        }
    }
}