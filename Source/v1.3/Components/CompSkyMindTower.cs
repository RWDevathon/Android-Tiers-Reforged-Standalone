﻿using Verse;
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

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);

            // Dead pawns that were acting as towers can no longer do so. Buildings do not call this function.
            Utils.gameComp.RemoveTower(this);
        }
        
        // After despawning remove the tower.
        public override void PostDeSpawn(Map map)
        { 
            base.PostDeSpawn(map);
            // If the tower has a power supply, then it will remove the tower capacity IF it was not offline to avoid double-reducing the tower capacity. This will not affect pawns so they can go caravanning.
            CompPowerTrader cpt = parent.TryGetComp<CompPowerTrader>();
            if (cpt != null && cpt.PowerOn)
            { 
                Utils.gameComp.RemoveTower(this);
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            switch (signal)
            {
                case "PowerTurnedOn":
                    Utils.gameComp.AddTower(this);
                    break;
                case "PowerTurnedOff":
                    Utils.gameComp.RemoveTower(this);
                    break;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            if (parent.Map == null)
                return base.CompInspectStringExtra();

            ret.Append("ATR_SkyMindNetworkSummary".Translate(Utils.gameComp.GetSkyMindDevices().Count, Utils.gameComp.GetSkyMindNetworkSlots()));

            return ret.Append(base.CompInspectStringExtra()).ToString();
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
                Utils.gameComp.AddTower(this);
            }
        }
    }
}