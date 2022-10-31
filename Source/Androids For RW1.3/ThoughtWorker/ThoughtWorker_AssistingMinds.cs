using System;
using System.Linq;
using RimWorld;
using Verse;

namespace ATReforged
{
    public class ThoughtWorker_AssistedByMinds : ThoughtWorker
    {
     
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            // Skip pawns that can't connect to the SkyMind.
            if (!Utils.PawnCanUseSkyMind(p))
                return false;

            // Skip pawns that can be but aren't connected to the SkyMind
            if (!Utils.GCATPP.HasSkyMindConnection(p))
            {
                return false;
            }

            int num = Utils.GCATPP.GetCloudPawns().Where(pawn => !pawn.health.hediffSet.hediffs.Any(hediff => hediff.def == HediffDefOf.ATR_MindOperation) && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()).Count();
            if (num >= 15)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            else if (num >= 5)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (num > 0)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return false;
        }
    }
}
