using System;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;
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

            if (respawningAfterLoad)
            {
                if (parent.TryGetComp<CompPowerTrader>().PowerOn)
                    StartSustainer();
            }
            else
            {
                serverMode = Props.serverMode;
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "ScheduledOff" || signal == "Breakdown" || signal == "PowerTurnedOff")
            {
                Utils.GCATPP.RemoveServer(building, serverMode, Props.pointStorage);
                StopSustainer();
            }

            if (signal == "PowerTurnedOn")
            {
                Utils.GCATPP.AddServer(building, serverMode, Props.pointStorage);
                StartSustainer();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (building.IsBrokenDown() || !parent.TryGetComp<CompPowerTrader>().PowerOn)
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
                    break;
                default:
                    Log.Warning("[ATR] Server has illegal type. Button will link to skill seeking to resolve the issue.");
                    yield return new Command_Action
                    { // In an illegal Mode, can switch to Skill
                        icon = Tex.SkillIcon,
                        defaultLabel = "ATR_SwitchToSkillMode".Translate(),
                        defaultDesc = "ATR_SwitchToSkillModeDesc".Translate(),
                        action = delegate ()
                        {
                            serverMode = ServerType.SkillServer;
                            Utils.GCATPP.AddServer(building, serverMode, Props.pointStorage);
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

            if (serverMode == ServerType.SkillServer)
            {
                ret.AppendLine("ATR_SkillServersSynthesis".Translate(Utils.GCATPP.GetSkillPoints(), Utils.GCATPP.GetSkillPointCapacity()))
                   .AppendLine("ATR_SkillProducedPoints".Translate(Props.passivePointGeneration))
                   .AppendLine("ATR_SkillSlotsAdded".Translate(Props.pointStorage));
            }

            if (serverMode == ServerType.SecurityServer)
            {
                ret.AppendLine("ATR_SecurityServersSynthesis".Translate(Utils.GCATPP.GetSecurityPoints(), Utils.GCATPP.GetSecurityPointCapacity()))
                   .AppendLine("ATR_SecurityProducedPoints".Translate(Props.passivePointGeneration))
                   .AppendLine("ATR_SecuritySlotsAdded".Translate(Props.pointStorage));
            }

            if (serverMode == ServerType.HackingServer)
            {
                ret.AppendLine("ATR_HackingServersSynthesis".Translate(Utils.GCATPP.GetHackingPoints(), Utils.GCATPP.GetHackingPointCapacity()))
                   .AppendLine("ATR_HackingProducedPoints".Translate(Props.passivePointGeneration))
                   .AppendLine("ATR_HackingSlotsAdded".Translate(Props.pointStorage));
            }
            return ret.TrimEnd().ToString();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            StopSustainer();

            // Only servers with types get removed from the lists
            if (serverMode != ServerType.None && !building.IsBrokenDown() && parent.TryGetComp<CompPowerTrader>().PowerOn)
                Utils.GCATPP.RemoveServer(building, serverMode, Props.pointStorage);
        }

        private void StartSustainer()
        {
            if (sustainer == null && Props.ambiance != "None" && !ATReforged_Settings.disableServersAmbiance)
            {
                SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.None);
                sustainer = SoundDef.Named(Props.ambiance).TrySpawnSustainer(info);
            }
        }

        private void StopSustainer()
        {
            if (sustainer != null && Props.ambiance != "None")
            {
                sustainer.End();
                sustainer = null;
            }
        }

        public void ChangeServerMode(ServerType newMode)
        {
            try
            {
                Utils.GCATPP.RemoveServer(building, serverMode, Props.pointStorage);
                Utils.GCATPP.AddServer(building, newMode, Props.pointStorage);
                serverMode = newMode;
            }
            catch (Exception ex)
            {
                Log.Error("[ATR] Unable to change the server mode of building " + parent.def.defName + " to the new server type! Error: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private Sustainer sustainer;
        private Building building;
        private ServerType serverMode = ServerType.SkillServer;
    }
}
