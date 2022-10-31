using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ATReforged
{
    public class Hediff_AssistingMinds : HediffWithComps
    { 
        public override bool ShouldRemove
        {
            get
            { // This hediff is added and removed only by the CompSkyMind.
                return false;
            }
        }

        public override int CurStageIndex
        { // Calculate the current stage. 15+ minds is stage 2, 5+ is stage 1, less is stage 0.
            get
            {
                int minds = Utils.GCATPP.GetCloudPawns().Where(pawn => !pawn.health.hediffSet.hediffs.Where(hediff => hediff.def == HediffDefOf.ATR_MindOperation).Any() && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()).Count();
                if (minds >= 15)
                    return 2;
                if (minds >= 5)
                    return 1;
                return 0;
            }
        }


        public override bool Visible
        {
            get
            {
                return true;
            }
        }
    }
}
