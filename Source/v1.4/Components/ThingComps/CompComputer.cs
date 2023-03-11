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
            networkConnection = parent.GetComp<CompSkyMind>();
            powerConnection = parent.GetComp<CompPowerTrader>();

            if (networkConnection == null || powerConnection == null)
            {
                Log.Error("[ATR] " + parent + " is missing a SkyMind or PowerTrader Comp! This means the gizmos/inspect pane are going to break! Report which object is responsible for this to its mod author.");
            }

            if (!respawningAfterLoad)
            {
                serverMode = Props.serverMode;
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "PowerTurnedOff" || signal == "SkyMindNetworkUserDisconnected")
            {
                Utils.gameComp.RemoveServer(building, serverMode);
            }
            else if ((signal == "SkyMindNetworkUserConnected" && powerConnection.PowerOn) || (signal == "PowerTurnedOn" && networkConnection?.connected != false))
            {
                UpdateGlow();
                Utils.gameComp.AddServer(building, serverMode);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!powerConnection.PowerOn || networkConnection?.connected == false)
                yield break;

            // Generate button to switch server mode based on which servermode the server is currently in.
            switch (serverMode)
            {
                // In Skill Mode, can switch to Security
                case ServerType.SkillServer:
                    yield return new Command_Action
                    {
                        icon = Tex.SkillIcon,
                        defaultLabel = "ATR_SkillMode".Translate(),
                        defaultDesc = "ATR_SkillModeDesc".Translate(),
                        action = delegate ()
                        {
                            ChangeServerMode(ServerType.SecurityServer);
                        }
                    };
                    break;
                // In Security Mode, can switch to Hacking
                case ServerType.SecurityServer:
                    yield return new Command_Action
                    {
                        icon = Tex.SecurityIcon,
                        defaultLabel = "ATR_SecurityMode".Translate(),
                        defaultDesc = "ATR_SecurityModeDesc".Translate(),
                        action = delegate ()
                        {
                            ChangeServerMode(ServerType.HackingServer);
                        }
                    };
                    break;
                // In Hacking Mode, can switch to Skill
                case ServerType.HackingServer:
                    yield return new Command_Action
                    {
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
                // In an illegal Mode, can switch to Skill
                default:
                    yield return new Command_Action
                    { 
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
            if (!powerConnection.PowerOn)
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

        // Change this server to the given type, making sure it deregisters from the previous type.
        public void ChangeServerMode(ServerType newMode)
        {
            Utils.gameComp.RemoveServer(building, serverMode);
            Utils.gameComp.AddServer(building, newMode);
            serverMode = newMode;
            UpdateGlow();
        }

        // Change the color of the server's glow to match its server type: green for skill, blue for security, red for hacking, black for illegal states. Do nothing if there is no CompGlower.
        private void UpdateGlow()
        {
            CompGlower glower = parent.GetComp<CompGlower>();
            if (glower == null)
            {
                return;
            }

            switch (serverMode)
            {
                case ServerType.SkillServer:
                {
                    glower.GlowColor = new ColorInt(0, 200, 0);
                    break;
                }
                case ServerType.SecurityServer:
                {
                    glower.GlowColor = new ColorInt(0, 0, 200);
                    break;
                }
                case ServerType.HackingServer:
                {
                    glower.GlowColor = new ColorInt(200, 0, 0);
                    break;
                }
                default:
                {
                    glower.GlowColor = new ColorInt(0, 0, 0);
                    break;
                }
            }
        }

        private CompSkyMind networkConnection;
        private CompPowerTrader powerConnection;
        private Building building;
        private ServerType serverMode = ServerType.SkillServer;
    }
}
