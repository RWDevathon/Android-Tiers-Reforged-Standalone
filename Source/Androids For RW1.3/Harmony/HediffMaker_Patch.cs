using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ATReforged
{
    internal class HediffMaker_Patch

    {
        [HarmonyPatch(new Type[] { typeof(HediffDef), typeof(Pawn), typeof(BodyPartRecord) })]
        [HarmonyPatch(typeof(HediffMaker), "MakeHediff")]
        public class MakeHediff_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(HediffDef def, Pawn pawn, BodyPartRecord partRecord, ref Hediff __result)
            {
                if (def == null)
                {
                    __result = null;
                    return false;
                }

                if (pawn == null)
                {
                    Log.Error("Cannot make hediff " + def + " for null pawn.");
                    __result = null;
                }
                //Log.Message("==>Attempt creating Hediff " + def.defName);
                Hediff hediff = (Hediff)Activator.CreateInstance(def.hediffClass);
                /*if (hediff != null)
                    Log.Message("==>Hediff created !!");
                else
                    Log.Message("==>Hediff failed to be created !!");*/
                hediff.def = def;
                hediff.pawn = pawn;
                hediff.Part = partRecord;
                hediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
                hediff.PostMake();
                __result = hediff;

                return false;
            }
        }
    }
}