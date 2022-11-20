using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class RestUtility_Patch
    {
        // Mechanicals that can charge will only consider active charging beds as valid, unless there are no such available charging beds.
        [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
        public class IsValidBedFor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Thing bedThing, Pawn sleeper, Pawn traveler, ref bool __result)
            {
                if (!__result)
                    return;

                if (Utils.IsConsideredMechanical(sleeper) && Utils.CanUseBattery(sleeper) && Utils.GetAvailableChargingBed(sleeper) != null &&
                    (bedThing.TryGetComp<CompPowerTrader>() == null || !bedThing.TryGetComp<CompPowerTrader>().PowerOn || bedThing.IsBrokenDown()))
                {
                    __result = false;
                }
            }
        }
    }
}