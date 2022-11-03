using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Sound;
using Verse;

namespace ATReforged
{
    public class CompSuperComputer : ThingComp
    {
        public CompProperties_SuperComputer Props
        {
            get
            {
                return (CompProperties_SuperComputer)props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            building = (Building)parent;

            if (Props.ambiance != "None")
            {
                ambiance = SoundDef.Named(Props.ambiance);
            }

            // The server lists need to know how much storage and point generation exists for each server mode. This adds it to all three types. It won't double-add if it was already contained.
            Utils.GCATPP.AddServer(building, Props.pointStorage);

            if (respawningAfterLoad)
            {
                StartSustainer();
            }
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            Utils.GCATPP.ChangeServerPoints(Props.passivePointGeneration, ServerType.SkillServer);
            Utils.GCATPP.ChangeServerPoints(Props.passivePointGeneration, ServerType.SecurityServer);
            Utils.GCATPP.ChangeServerPoints(Props.passivePointGeneration, ServerType.HackingServer);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            ret.AppendLine("ATR_SkillServersSynthesis".Translate(Utils.GCATPP.GetSkillPoints(), Utils.GCATPP.GetSkillPointCapacity()))
               .AppendLine("ATR_SecurityServersSynthesis".Translate(Utils.GCATPP.GetSecurityPoints(), Utils.GCATPP.GetSecurityPointCapacity()))
               .AppendLine("ATR_HackingServersSynthesis".Translate(Utils.GCATPP.GetHackingPoints(), Utils.GCATPP.GetHackingPointCapacity()))
               .AppendLine("ATR_SkillProducedPoints".Translate(Props.passivePointGeneration))
               .AppendLine("ATR_SecurityProducedPoints".Translate(Props.passivePointGeneration))
               .AppendLine("ATR_HackingProducedPoints".Translate(Props.passivePointGeneration))
               .AppendLine("ATR_SkillSlotsAdded".Translate(Props.pointStorage))
               .AppendLine("ATR_SecuritySlotsAdded".Translate(Props.pointStorage))
               .AppendLine("ATR_HackingSlotsAdded".Translate(Props.pointStorage));
            return ret.ToString();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            StopSustainer();

            // The server lists need to know how much storage exists for each server mode. This removes it from all three types.
            Utils.GCATPP.RemoveServer(building, Props.pointStorage);
        }


        private void StartSustainer()
        {
            if (sustainer == null && Props.ambiance != "None" && !ATReforged_Settings.disableServersAmbiance)
            {
                SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.None);
                sustainer = ambiance.TrySpawnSustainer(info);
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

        private Sustainer sustainer;
        private SoundDef ambiance;
        private Building building = null;
    }
}
