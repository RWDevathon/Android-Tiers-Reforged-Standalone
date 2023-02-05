using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace ATReforged
{
    internal static class FlickUtility_Patch
    {
        // Ensure the SkyMind auto-flicks things when requested and connected.
        [HarmonyPatch(typeof(FlickUtility), "UpdateFlickDesignation")]
        public class UpdateFlickDesignation_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Thing t)
            {
                if (t.TryGetComp<CompSkyMind>()?.connected == true && Utils.gameComp.GetSkyMindCloudCapacity() > 0)
                {
                    CompFlickable compFlick = t.TryGetComp<CompFlickable>();
                    if (compFlick != null)
                    {
                        string txt;
                        if (compFlick.SwitchIsOn)
                        {
                            txt = "ATR_FlickDisable".Translate();
                        }
                        else
                        {
                            txt = "ATR_FlickEnable".Translate();
                        }

                        MoteMaker.ThrowText(t.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), t.Map, txt, Color.white, -1f);

                        compFlick.DoFlick();
                    }
                    return false;
                }
                return true;
            }
        }
    }
}