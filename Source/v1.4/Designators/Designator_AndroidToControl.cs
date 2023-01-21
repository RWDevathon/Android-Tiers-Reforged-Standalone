using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Designator_AndroidToControl : Designator
    {
        public Designator_AndroidToControl(Pawn controller)
        {
            defaultLabel = "ATR_ControlSurrogate".Translate();
            defaultDesc = "ATR_ControlSurrogateDesc".Translate();
            soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            soundDragChanged = null;
            soundSucceeded = SoundDefOf.Designate_ZoneDelete;
            useMouseIcon = true;
            icon = Tex.ConnectIcon;
            hotKey = KeyBindingDefOf.Misc4;
            this.controller = controller;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
            {
                return false;
            }

            foreach (Thing thing in c.GetThingList(Map))
            {
                if (CanDesignateThing(thing).Accepted)
                    return true;
            }

            return "ATR_NoAvailableTarget".Translate();
        }

        public override void DesignateSingleCell(IntVec3 c)
        {

        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            base.RenderHighlight(dragCells);

            DesignatorUtility.RenderHighlightOverSelectableThings(this, dragCells);
        }


        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            base.CanDesignateThing(thing);
            if (thing is Pawn pawn)
            {
                // Can only designate player (or blank) connected, unbreached, uncontrolled surrogates.
                if ((pawn.Faction == null || pawn.Faction.IsPlayer) && Utils.IsSurrogate(pawn) 
                    && pawn.GetComp<CompSkyMind>().Breached == -1 && !pawn.GetComp<CompSkyMindLink>().HasSurrogate())
                {
                    target = pawn;
                    return true;
                }
            }
            return false;
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();

            if (!Utils.gameComp.AttemptSkyMindConnection((Pawn)target))
            { // If trying to connect but it is unable to, inform the player. 
                if (Utils.gameComp.GetSkyMindNetworkSlots() == 0)
                    Messages.Message("ATR_SkyMindConnectionFailedNoNetwork".Translate(), target, MessageTypeDefOf.NegativeEvent);
                else
                    Messages.Message("ATR_SkyMindConnectionFailed".Translate(), target, MessageTypeDefOf.NegativeEvent);
                return;
            }

            controller.GetComp<CompSkyMindLink>().ConnectSurrogate((Pawn)target);
            if (Utils.gameComp.GetSkyMindNetworkSlots() <= Utils.gameComp.networkedDevices.Count())
                Find.DesignatorManager.Deselect();
        }

        private Thing target;
        private Pawn controller;
    }
}
