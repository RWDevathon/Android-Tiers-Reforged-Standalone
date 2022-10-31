using Verse;
using System.Collections.Generic;

namespace ATReforged
{
    public class CompPawnBattery : ThingComp
    {
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref useBattery, "ATR_useBattery", ATReforged_Settings.useBatteryByDefault, true);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                if (!Utils.CanUseBattery((Pawn) parent))
                    useBattery = false;
                else
                    useBattery = ATReforged_Settings.useBatteryByDefault;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn pawn = (Pawn)parent;
            if (Utils.CanUseBattery(pawn))
            {
                yield return new Command_Toggle
                {
                    icon = Tex.Battery,
                    defaultLabel = "ATR_UseBattery".Translate(),
                    defaultDesc = "ATR_UseBatteryDesc".Translate(),
                    isActive = (() => useBattery),
                    toggleAction = delegate ()
                    {
                        useBattery = !useBattery;
                    }
                };
            }
            yield break;
        }

        public bool UseBattery
        {
            get
            {
                return useBattery;
            }
        }

        public bool useBattery = false;
    }
}