using Verse;
using HarmonyLib;
using RimWorld;
using System;

namespace ATReforged
{
    internal class PawnGenerator_Patch

    {
        // Patch pawn generation for mechanical units so they have appropriate gender, name, features, and various related mechanics at generation.
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
                    BodyTypeDef bodyDef;
                    HeadTypeDef headDef;


                    if (__result.gender == Gender.Male)
                    {
                        bodyDef = DefDatabase<BodyTypeDef>.GetNamed("Male", false);
                        headDef = DefDatabase<HeadTypeDef>.GetNamed("ATR_Male_Average_Normal", false);
                    }
                    else if (__result.gender == Gender.Female)
                    {
                        bodyDef = DefDatabase<BodyTypeDef>.GetNamed("Female", false);
                        headDef = DefDatabase<HeadTypeDef>.GetNamed("ATR_Female_Average_Normal", false);
                    }
                    else
                    {
                        bodyDef = DefDatabase<BodyTypeDef>.GetNamed("None", false);
                        headDef = DefDatabase<HeadTypeDef>.GetNamed("ATR_None_Average_Normal", false);
                    }
                    if (bodyDef != null)
                        __result.story.bodyType = bodyDef;
                    if (headDef != null)
                        __result.story.headType = headDef;

                    Utils.removeMindBlacklistedTrait(__result);
                    Utils.ReconfigureDrone(__result);

                    // By default, all generated mechanical androids that reach this point get an autonomous core. Cases where that is not desired can remove it there.
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