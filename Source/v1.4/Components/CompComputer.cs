using System;
using System.Text;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    public class CompComputer : ThingComp
    {
        public CompProperties_Computer Props
        {
            get
            {
                return (CompProperties_Computer)props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref serverMode, "ATR_serverMode", ServerType.SkillServer);
        }

        // There are two possible spawn states: created, in which case it sets its serverMode from Props and waits to turn on; post load spawn, in which case it already has a mode and state.
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            building = (Building)parent;
            networkConnection = parent.TryGetComp<CompSkyMind>();

            if (!respawningAfterLoad)
            {
                serverMode = Props.serverMode;
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "ScheduledOff" || signal == "Breakdown" || signal == "PowerTurnedOff" || signal == "SkyMindNetworkUserDisconnected")
            {
                Utils.gameComp.RemoveServer(building, serverMode);
            }
            else if ((signal == "SkyMindNetworkUserConnected" && parent.TryGetComp<CompPowerTrader>().PowerOn) || (signal == "PowerTurnedOn" && networkConnection?.connected == true))
            {
                Utils.gameComp.AddServer(building, serverMode);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (building.IsBrokenDown() || !parent.TryGetComp<CompPowerTrader>().PowerOn || networkConnection?.connected == false)
                yield break;

            // Generate button to switch server mode based on which servermode the server is currently in.
            switch (serverMode)
            {
                case ServerType.None:
                    yield break;
                case ServerType.SkillServer:
                    yield return new Command_Action
                    { // In Skill Mode, can switch to Security
                        icon = Tex.SkillIcon,
                        defaultLabel = "ATR_SkillMode".Translate(),
                        defaultDesc = "ATR_SkillModeDesc".Translate(),
                        action = delegate ()
                        {
                            ChangeServerMode(ServerType.SecurityServer);
                        }
                    };
                    break;
                case ServerType.SecurityServer:
                    yield return new Command_Action
                    { // In Security Mode, can switch to Hacking
                        icon = Tex.SecurityIcon,
                        defaultLabel = "ATR_SecurityMode".Translate(),
                        defaultDesc = "ATR_SecurityModeDesc".Translate(),
                        action = delegate ()
                        {
                            ChangeServerMode(ServerType.HackingServer);
                        }
                    };
                    break;
                case ServerType.HackingServer:
                    yield return new Command_Action
                    { // In Hacking Mode, can switch to Skill
                        icon = Tex.HackingIcon,
                        defaultLabel = "ATR_HackingMode".Translate(),
                        defaultDesc = "ATR_HackingModeDesc".Translate(),
                        action = delegate ()
                        {
                            ChangeServerMode(ServerType.SkillServer);
                        }
                    };

                    // Servers in hacking mode allow access to the hacking menu for deploying a hack.
                    if (ATReforged_Settings.playerCanHack)
                    {
                        yield return new Command_Action
                        {
                            icon = Tex.HackingWindowIcon,
                            defaultLabel = "ATR_HackingWindow".Translate(),
                            defaultDesc = "ATR_HackingWindowDesc".Translate(),
                            action = delegate ()
                            {
                                Find.WindowStack.Add(new Dialog_HackingWindow());
                            }
                        };
                    }
                    break;
                default:
                    yield return new Command_Action
                    { // In an illegal Mode, can switch to Skill
                        icon = Tex.SkillIcon,
                        defaultLabel = "ATR_SwitchToSkillMode".Translate(),
                        defaultDesc = "ATR_SwitchToSkillModeDesc".Translate(),
                        action = delegate ()
                        {
                            serverMode = ServerType.SkillServer;
                            Utils.gameComp.AddServer(building, serverMode);
                        }
                    };
                    break;
            }
        }
        
        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();
            if (building.IsBrokenDown() || !parent.TryGetComp<CompPowerTrader>().PowerOn)
                return "";

            if (networkConnection?.connected == false)
            {
                ret.Append("ATR_ServerNetworkConnectionNeeded".Translate());
                return ret.Append(base.CompInspectStringExtra()).ToString();
            }

            if (serverMode == ServerType.SkillServer)
            {
                ret.AppendLine("ATR_SkillServersSynthesis".Translate(Utils.gameComp.GetPoints(ServerType.SkillServer), Utils.gameComp.GetPointCapacity(ServerType.SkillServer)))
                   .AppendLine("ATR_SkillProducedPoints".Translate(Props.passivePointGeneration))
                   .Append("ATR_SkillSlotsAdded".Translate(Props.pointStorage));
            }
            else if (serverMode == ServerType.SecurityServer)
            {
                ret.AppendLine("ATR_SecurityServersSynthesis".Translate(Utils.gameComp.GetPoints(ServerType.SecurityServer), Utils.gameComp.GetPointCapacity(ServerType.SecurityServer)))
                   .AppendLine("ATR_SecurityProducedPoints".Translate(Props.passivePointGeneration))
                   .Append("ATR_SecuritySlotsAdded".Translate(Props.pointStorage));
            }
            else if (serverMode == ServerType.HackingServer)
            {
                ret.AppendLine("ATR_HackingServersSynthesis".Translate(Utils.gameComp.GetPoints(ServerType.HackingServer), Utils.gameComp.GetPointCapacity(ServerType.HackingServer)))
                   .AppendLine("ATR_HackingProducedPoints".Translate(Props.passivePointGeneration))
                   .Append("ATR_HackingSlotsAdded".Translate(Props.pointStorage));
            }
            return ret.Append(base.CompInspectStringExtra()).ToString();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            // Servers can not be connected to the network when despawned.
            if (networkConnection?.connected == true)
            {
                Utils.gameComp.DisconnectFromSkyMind(building);
            }
        }

        public void ChangeServerMode(ServerType newMode)
        {
            try
            {
                Utils.gameComp.RemoveServer(building, serverMode);
                Utils.gameComp.AddServer(building, newMode);
                serverMode = newMode;
            }
            catch (Exception ex)
            {
                Log.Error("[ATR] Unable to change the server mode of building " + parent.def.defName + " to the new server type " + newMode + "! Error: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private CompSkyMind networkConnection;
        private Building building;
        private ServerType serverMode = ServerType.SkillServer;
    }
}
