using Verse;
using HarmonyLib;
using RimWorld;

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

                if (Utils.IsConsideredMechanical(sleeper) && Utils.CanUseBattery(sleeper) &&
                    (bedThing.TryGetComp<CompPowerTrader>() == null || !bedThing.TryGetComp<CompPowerTrader>().PowerOn || bedThing.IsBrokenDown()))
                {
                    __result = false;
                }
            }
        }
    }
}