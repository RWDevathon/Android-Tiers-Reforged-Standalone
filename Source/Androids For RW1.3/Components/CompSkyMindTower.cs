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
            Utils.GCATPP.RemoveTower(this);
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

            CompPowerTrader cpt = parent.TryGetComp<CompPowerTrader>();
            if (cpt == null)
            { // If there is no power supply to this server, it can't be turned off normally. Just add it in and handle removing it separately.
                Utils.GCATPP.AddTower(this);
            }
            else if (cpt.PowerOn)
            { // If it does have a power supply, make sure it's on before adding it into the list.
                Utils.GCATPP.AddTower(this);
            }
        }
    }
}