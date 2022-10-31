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
    internal class StunHandler_Patch

    {

        [HarmonyPatch(typeof(StunHandler), "get_AffectedByEMP")]
        public class get_AffectedByEMP_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Thing ___parent)
            {
                if (___parent is Pawn)
                {
                    Pawn pawn = (Pawn)___parent;
                    if (Utils.IsConsideredMechanical(pawn))
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}