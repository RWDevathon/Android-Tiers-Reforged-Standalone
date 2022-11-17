using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class RestUtility_Patch
    {
        // Mechanicals that can charge can ONLY use charge-capable beds.
        [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
        public class IsValidBedFor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Thing bedThing, Pawn sleeper, Pawn traveler, ref bool __result)
            {
                if (!__result)
                    return;

                if (Utils.IsConsideredMechanical(sleeper) && Utils.CanUseBattery(sleeper) && bedThing.TryGetComp<CompPowerTrader>() == null)
                {
                    __result = false;
                }
            }
        }
    }
}