using RimWorld;
using UnityEngine;
using Verse;

namespace ATReforged
{
    // This HediffComp will create and manage a Mote to display over the related pawn's head as long as the Comp persists.
    public class HediffComp_TetheredMoteSustainer : HediffComp
    {
        public HediffCompProperties_TetheredMoteSustainer Props => (HediffCompProperties_TetheredMoteSustainer)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            if (Pawn.Spawned && ATReforged_Settings.displaySurrogateControlIcon)
            {
                AssignAttachedMote();
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (attachedMote == null && Pawn.Spawned && ATReforged_Settings.displaySurrogateControlIcon)
            {
                AssignAttachedMote();
            }
            attachedMote?.Maintain();
        }

        public void AssignAttachedMote()
        {
            Vector3 vector = Vector3.zero;
            vector.x += Pawn.def.size.x / 2;
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28125f;
            vector.z += 1.4f;
            attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.moteDef, vector, Props.scale);
        }

        private Mote attachedMote;
    }
}
