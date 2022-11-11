using System.Collections.Generic;
using System.Text;
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

            // The server lists need to know how much storage and point generation exists for each server mode. This adds it to all three types.
            if (!respawningAfterLoad)
                Utils.gameComp.AddServer(building, Props.pointStorage);
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            Utils.gameComp.ChangeServerPoints(Props.passivePointGeneration, ServerType.SkillServer);
            Utils.gameComp.ChangeServerPoints(Props.passivePointGeneration, ServerType.SecurityServer);
            Utils.gameComp.ChangeServerPoints(Props.passivePointGeneration, ServerType.HackingServer);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Servers in hacking mode allow access to the hacking menu for deploying a hack. Supercomputers always enable hacking mode, so they always have the gizmo to open the menu.
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
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            ret.AppendLine("ATR_SkillServersSynthesis".Translate(Utils.gameComp.GetSkillPoints(), Utils.gameComp.GetSkillPointCapacity()))
               .AppendLine("ATR_SecurityServersSynthesis".Translate(Utils.gameComp.GetSecurityPoints(), Utils.gameComp.GetSecurityPointCapacity()))
               .AppendLine("ATR_HackingServersSynthesis".Translate(Utils.gameComp.GetHackingPoints(), Utils.gameComp.GetHackingPointCapacity()))
               .AppendLine("ATR_SkillProducedPoints".Translate(Props.passivePointGeneration))
               .AppendLine("ATR_SecurityProducedPoints".Translate(Props.passivePointGeneration))
               .AppendLine("ATR_HackingProducedPoints".Translate(Props.passivePointGeneration))
               .AppendLine("ATR_SkillSlotsAdded".Translate(Props.pointStorage))
               .AppendLine("ATR_SecuritySlotsAdded".Translate(Props.pointStorage))
               .Append("ATR_HackingSlotsAdded".Translate(Props.pointStorage));
            return ret.Append(base.CompInspectStringExtra()).ToString();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            // The server lists need to know how much storage exists for each server mode. This removes it from all three types.
            Utils.gameComp.RemoveServer(building, Props.pointStorage);
        }

        private Building building = null;
    }
}
