using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace ATReforged
{
    public class CompHeatSensitive : ThingComp
    {
        public CompProperties_HeatSensitive Props
        {
            get
            {
                return (CompProperties_HeatSensitive)props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref heatLevel, "ATR_heatLevel", 0, false);
            Scribe_Values.Look(ref checksSinceCritical, "ATR_checksSinceCritical", 0, false);
        }

        public override void PostDraw()
        {
            Material iconMat = null;

            if (heatLevel == 0)
            {
                return;
            }

            if (heatLevel == 1)
                iconMat = Tex.WarningHeat;
            else if (heatLevel == 2)
                iconMat = Tex.DangerHeat;
            else if (heatLevel == 3)
                iconMat = Tex.CriticalHeat;

            Vector3 vector = parent.TrueCenter();
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28125f;
            vector.x += parent.def.size.x / 4;

            vector.z -= 1;

            var num = (Time.realtimeSinceStartup + 397f * (parent.thingIDNumber % 571)) * 4f;
            var num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;
            var material = FadedMaterialPool.FadedVersionOf(iconMat, num2);
            Graphics.DrawMesh(MeshPool.plane05, vector, Quaternion.identity, material, 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            active = parent.GetComp<CompPowerTrader>()?.PowerOn != false; // If the Power Trader is on or null, heat sensitive device is active.
        }

        // Checker for heat on normal Ticks. 250x reduced impact to check heat damage.
        public override void CompTick()
        {
            base.CompTick();

            CheckTemperature(0);
        }

        // Checker for heat on Rare Ticks (250 Ticks). Default checker to check heat damage.
        public override void CompTickRare()
        {
            base.CompTickRare();

            CheckTemperature(1);
        }

        // Checker for heat on Long Ticks (2000 Ticks). 8x increased impact to check heat damage.
        public override void CompTickLong()
        {
            base.CompTickLong();

            CheckTemperature(2);
        }

        public override void ReceiveCompSignal(string signal)
        {
            // Terminate heat level if the server is offline
            if (signal == "PowerTurnedOff")
            {
                active = false;
                heatLevel = 0;
                checksSinceCritical = 0;
            }
            else if (signal == "PowerTurnedOn")
            {
                active = true;
            }
        }

        private void CheckTemperature(int tickerType)
        {
            if (!active)
            {
                return;
            }

            float ambientTemperature = parent.AmbientTemperature;

            // Ensure the device is sitting in the correct heat level index. 0 : safe, 1 : warning, 2 : danger, 3 : critical
            if (ambientTemperature >= Props.dangerHeat)
            {
                heatLevel = 3;

                if (lastTickSentCriticalHeat + 8000 < Find.TickManager.TicksGame)
                {
                    lastTickSentCriticalHeat = Find.TickManager.TicksGame;
                    Messages.Message("ATR_AlertServerHeatCriticalDesc".Translate(), new LookTargets(parent), MessageTypeDefOf.NegativeEvent, false);
                }

                switch (tickerType)
                {
                    case 0:
                        checksSinceCritical++;
                        break;
                    case 1:
                        checksSinceCritical += 250;
                        break;
                    case 2:
                        checksSinceCritical += 2000;
                        break;
                }
            }
            else
            {
                checksSinceCritical = 0;
                if (ambientTemperature >= Props.warningHeat)
                {
                    heatLevel = 2;
                }
                else if (ambientTemperature >= Props.safeHeat)
                {
                    heatLevel = 1;
                }
                else
                {
                    heatLevel = 0;
                }
            }

            // Generate explosion, reset explosion checker. Chance to explode starts at 10% at the 11th check (if Rare is used), with each additional check getting +10% chance (100% at 20 checks).
            if (checksSinceCritical >= 3750 + Rand.RangeInclusive(-1000, 1250))
            {
                checksSinceCritical = 0;

                MakeExplosion();
                Find.LetterStack.ReceiveLetter("ATR_ServerMeltdown".Translate(), "ATR_ServerMeltdownDesc".Translate(), LetterDefOf.NegativeEvent, new TargetInfo(parent.Position, parent.Map, false), null, null);
            }
        }

        public void MakeExplosion()
        {
            CompBreakdownable breakComp = parent.GetComp<CompBreakdownable>();
            if (breakComp != null)
                breakComp.DoBreakdown();

            Building building = (Building)parent;
            building.HitPoints -= (int)(building.HitPoints * Rand.Range(0.10f, 0.45f));

            GenExplosion.DoExplosion(parent.Position, parent.Map, 3, DamageDefOf.Flame, null);
        }

        public override string CompInspectStringExtra()
        {
            if (!active)
                return "";

            if (heatLevel == 3)
                return "ATR_CompHotSensitiveCriticalText".Translate();
            else if (heatLevel == 2)
                return "ATR_CompHotSensitiveDangerText".Translate();
            else if (heatLevel == 1)
                return "ATR_CompHotSensitiveWarningText".Translate();
            else
                return "ATR_CompHotSensitiveSafeText".Translate();
        }

        private bool active;

        private int heatLevel;

        private int checksSinceCritical = 0;

        private static int lastTickSentCriticalHeat = 0;
    }
}
