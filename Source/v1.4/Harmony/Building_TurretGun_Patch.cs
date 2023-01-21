using HarmonyLib;
using RimWorld;
using Verse;

namespace ATReforged
{
    // Allow manual control of SkyMind connected turrets.
    [HarmonyPatch(typeof(Building_TurretGun), "get_CanSetForcedTarget")]
    public static class CanSetForcedTarget_Patch
    {
        [HarmonyPostfix]
        public static void Listener(Building_TurretGun __instance, ref bool __result)
        {
            // No need to check anything if it's already true.
            if (__result)
                return;

            // If there is a SkyMind Core of any kind and this turret is linked to the SkyMind, then it may have a forced target.
            CompSkyMind compSkyMind = __instance.GetComp<CompSkyMind>();
            if (compSkyMind != null && compSkyMind.connected && Utils.gameComp.GetSkyMindCloudCapacity() > 0)
                __result = true;
        }
    }
}
