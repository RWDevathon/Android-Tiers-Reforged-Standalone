using Verse;
using RimWorld;
using System.Text;

namespace ATReforged
{
    public class CompSkyMindTower : ThingComp
    {
        public CompProperties_SkyMindTower Props
        {
            get
            {
                return (CompProperties_SkyMindTower)props;
            }
        }

        // After despawning remove the tower.
        public override void PostDeSpawn(Map map)
        { 
            base.PostDeSpawn(map);
            CompPowerTrader cpt = parent.TryGetComp<CompPowerTrader>();
            if (cpt == null)
            { // If there is no power supply to this server, then despawning is the only time it can turn off and drop capacity.
                Utils.GCATPP.RemoveTower(this);
            }
            else if (cpt.PowerOn)
            { // If it does have a power supply, then make sure the power is on before reducing capacity, as offline towers provide no capacity in the first place.
                Utils.GCATPP.RemoveTower(this);
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            switch (signal)
            {
                case "PowerTurnedOn":
                    Utils.GCATPP.AddTower(this);
                    break;
                case "PowerTurnedOff":
                    Utils.GCATPP.RemoveTower(this);
                    break;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            if (parent.Map == null)
                return base.CompInspectStringExtra();

            ret.AppendLine("ATR_SkyMindNetworkSummary".Translate(Utils.GCATPP.GetSkyMindDevices().Count, Utils.GCATPP.GetSkyMindNetworkSlots()));

            return ret.TrimEnd().Append(base.CompInspectStringExtra()).ToString();
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // No need to handle anything upon loading a save - capacity is saved in the GameComponent and we should avoid adding extra capacity.
            if (respawningAfterLoad)
                return;

            // If there is no power supply to this server, it can't be turned on/off normally. Just add it in and handle removing it separately.
            if (parent.TryGetComp<CompPowerTrader>() == null)
            { 
                Utils.GCATPP.AddTower(this);
            }
        }
    }
}