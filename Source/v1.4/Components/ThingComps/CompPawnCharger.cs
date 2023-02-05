using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ATReforged
{
    public class CompPawnCharger : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPowerTrader = parent.GetComp<CompPowerTrader>();

            if (!respawningAfterLoad)
            {
                cachedConsumption = compPowerTrader.Props.PowerConsumption;
                users = new List<Pawn>();
            }
            else if (users == null)
            {
                users = new List<Pawn>();
            }
            RecalculateConsumption();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref users, "ATR_chargingCompUsers", LookMode.Reference);
            Scribe_Values.Look(ref cachedConsumption, "ATR_chargingCompCachedConsumption");
        }

        public override void CompTick()
        {
            base.CompTick();
            UpdatePowerConsumption();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            UpdatePowerConsumption();
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            UpdatePowerConsumption();
        }

        public override string CompInspectStringExtra()
        {
            return "ATR_CurrentChargerPowerConsumption".Translate(cachedConsumption.ToString("#####0"));
        }

        private void UpdatePowerConsumption()
        {
            compPowerTrader.PowerOutput = -cachedConsumption;
        }

        // Inform this charger that a given pawn is now charging here.
        public void Notify_ConsumerAdded(Pawn pawn)
        {
            if (!users.Contains(pawn))
            {
                users.Add(pawn);
                RecalculateConsumption();
            }
        }

        // Inform this charger that a given pawn is no longer charging here.
        public void Notify_ConsumerRemoved(Pawn pawn)
        {
            if (users.Contains(pawn))
            {
                users.Remove(pawn);
                RecalculateConsumption();
            }
        }

        public void RecalculateConsumption()
        {
            if (users.Count == 0)
            {
                cachedConsumption = compPowerTrader.Props.PowerConsumption;
            }
            else
            {
                cachedConsumption = 0;
                foreach (Pawn user in users)
                {
                    cachedConsumption += user.BodySize * ATReforged_Settings.wattsConsumedPerBodySize;
                }
            }
            UpdatePowerConsumption();
        }

        protected CompPowerTrader compPowerTrader;
        protected List<Pawn> users;
        protected float cachedConsumption;
    }
}