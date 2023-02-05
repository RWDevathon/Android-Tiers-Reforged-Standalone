using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace ATReforged
{
    public class CompAutoDoor : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            doorRef = parent as Building_Door;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Do not show button to manually open/close door if the door isn't connected to the SkyMind or there is no SkyMind Cloud.
            if (parent.GetComp<CompSkyMind>()?.connected != true || Utils.gameComp.GetSkyMindCloudCapacity() == 0)
            {
                yield break;
            }

            // Do not show buttons if the door's power is off.
            if (!parent.GetComp<CompPowerTrader>().PowerOn)
            {
                yield break;
            }

            if (doorRef.Open)
            {
                yield return new Command_Action
                {
                    icon = Tex.CloseDoorIcon,
                    defaultLabel = "ATR_AutoDoorClose".Translate(),
                    defaultDesc = "ATR_AutoDoorCloseDescription".Translate(),
                    action = delegate ()
                    {
                        if (Traverse.Create(doorRef).Field("holdOpenInt").GetValue<bool>())
                            Traverse.Create(doorRef).Field("holdOpenInt").SetValue(false);

                        Traverse.Create(doorRef).Method("DoorTryClose", new object[0]).GetValue();
                        MoteMaker.ThrowText(doorRef.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), doorRef.Map, "ATR_AutoDoorCloseMoteText".Translate(), Color.white, -1f);
                    }
                };
            }
            else
            {
                yield return new Command_Action
                {
                    icon = Tex.OpenDoorIcon,
                    defaultLabel = "ATR_AutoDoorOpen".Translate(),
                    defaultDesc = "ATR_AutoDoorOpenDescription".Translate(),
                    action = delegate ()
                    {
                        if (!Traverse.Create(doorRef).Field("holdOpenInt").GetValue<bool>())
                            Traverse.Create(doorRef).Field("holdOpenInt").SetValue(true);

                        doorRef.StartManualOpenBy(null);
                        MoteMaker.ThrowText(doorRef.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), doorRef.Map, "ATR_AutoDoorOpenMoteText".Translate(), Color.white, -1f);
                    }
                };
            }
            yield break;
        }

        private Building_Door doorRef;
    }
}