using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

namespace ATReforged
{
    internal class PawnGenerator_Patch

    {
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn")]
        [HarmonyPatch(new Type[] { typeof(PawnGenerationRequest)}, new ArgumentType[] { ArgumentType.Normal })]
        public class GeneratePawn_Patch
        {
            [HarmonyPostfix]
            public static void Listener(PawnGenerationRequest request, ref Pawn __result)
            {
                try
                {
                    // Only mechanicals need to be patched.
                    if (!Utils.IsConsideredMechanical(__result) || Utils.HasSpecialStatus(__result) || Utils.IsConsideredMechanicalAnimal(__result))
                        return;


                    // Generate new gender according to settings.
                    __result.gender = Utils.GenerateGender(__result.kindDef);
                    __result.Name = PawnBioAndNameGenerator.GeneratePawnName(__result);

                    
                    if (__result.gender == Gender.Male)
                    {
                        BodyTypeDef bd = DefDatabase<BodyTypeDef>.GetNamed("Male", false);
                        if (bd != null)
                            __result.story.bodyType = bd;
                    }
                    else if (__result.gender == Gender.Female)
                    {
                        BodyTypeDef bd = DefDatabase<BodyTypeDef>.GetNamed("Female", false);
                        if (bd != null)
                            __result.story.bodyType = bd;
                    }
                    else
                    {
                        BodyTypeDef bd = DefDatabase<BodyTypeDef>.GetNamed("None", false);
                        if (bd != null)
                            __result.story.bodyType = bd;
                    }

                    Utils.removeMindBlacklistedTrait(__result);
                    Utils.ReconfigureDrone(__result);

                    // By default, all generated mechanical androids get an autonomous cores. Cases where that is not desired can remove it there.
                    if (Utils.IsConsideredMechanicalAndroid(__result))
                        __result.health.AddHediff(HediffDefOf.ATR_AutonomousCore, __result.health.hediffSet.GetBrain());
                }
                catch(Exception ex)
                {
                    Log.Error("[ATR] PawnGenerator.GeneratePawn " + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}