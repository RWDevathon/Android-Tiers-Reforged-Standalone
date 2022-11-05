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
        public class UpdateFlickDesignation
        {
            [HarmonyPrefix]
            public static bool UpdateFlickDesignation_Prefix(Thing t)
            {
                if (t.TryGetComp<CompSkyMind>() != null && t.TryGetComp<CompSkyMind>().connected && Utils.gameComp.GetSkyMindCloudCapacity() > 0)
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