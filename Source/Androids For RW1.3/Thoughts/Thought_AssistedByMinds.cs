using System;
using UnityEngine;
using System.Linq;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Thought_AssistedByMinds : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                return CurStage.label;
            }
        }

        protected override float BaseMoodOffset
        {
            get
            {
                int points = Utils.GCATPP.GetCloudPawns().Where(pawn => !pawn.health.hediffSet.hediffs.Any(hediff => hediff.def == HediffDefOf.ATR_MindOperation) && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate()).Count() * ATReforged_Settings.nbMoodPerAssistingMinds;
                return Mathf.Min(points, 10);
            }
        }
    }
}