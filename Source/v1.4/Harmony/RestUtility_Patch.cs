using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    internal class RestUtility_Patch
    {
        // Mechanicals that can charge will only consider active charging beds as valid.
        [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
        public class IsValidBedFor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Thing bedThing, Pawn sleeper, Pawn traveler, ref bool __result)
            {
                if (!__result)
                    return;

                if (Utils.IsConsideredMechanical(sleeper) && Utils.CanUseBattery(sleeper))
                {
                    Building_Bed building_Bed = bedThing as Building_Bed;

                    // Charging bed check (the building itself has a CompPawnCharger)
                    if (building_Bed != null && building_Bed.TryGetComp<CompPawnCharger>() != null && building_Bed.TryGetComp<CompPowerTrader>()?.PowerOn == true)
                    {
                        __result = true;
                        return;
                    }
                    
                    // Normal bed with an attached bedside charger check (A linked building has a CompPawnCharger)
                    List<Thing> linkedBuildings = building_Bed.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading;
                    if (linkedBuildings != null)
                    {
                        foreach (Thing linkedBuilding in linkedBuildings)
                        {
                            if (linkedBuilding.TryGetComp<CompPawnCharger>() != null && linkedBuilding.TryGetComp<CompPowerTrader>()?.PowerOn == true)
                            {
                                __result = true;
                                return;
                            }
                        }
                    }

                    __result = false;
                }
                // Organics without the ability to charge may not use charging beds.
                else if (!Utils.IsConsideredMechanical(sleeper) && !Utils.CanUseBattery(sleeper))
                {
                    if (bedThing is Building_ChargingBed)
                        __result = false;
                }
            }
        }
    }
}