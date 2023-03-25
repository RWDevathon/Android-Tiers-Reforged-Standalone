using Verse;
using HarmonyLib;
using System;

namespace ATReforged
{
    public class PawnGenerator_Patch
    {
        // Prefix pawn generation for mechanical units so they have the appropriate gender. This will allow vanilla pawn gen to handle various details like name and body type automatically.
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn")]
        [HarmonyPatch(new Type[] { typeof(PawnGenerationRequest) }, new ArgumentType[] { ArgumentType.Normal })]
        public class GeneratePawn_Prefix
        {
            [HarmonyPrefix]
            public static bool Prefix(ref PawnGenerationRequest request)
            {
                ThingDef targetDef = request.KindDef?.race;
                if (targetDef != null)
                {
                    if ((Utils.IsConsideredMechanicalAndroid(targetDef) || Utils.IsConsideredMechanicalDrone(targetDef)) && !Utils.HasSpecialStatus(targetDef))
                    {
                        request.FixedGender = Utils.GenerateGender(request.KindDef);
                    }
                }
                // This prefix will always allow vanilla pawn gen to continue
                return true;
            }
        }

        // Patch pawn generation for mechanical units so they have appropriate related mechanics at generation.
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn")]
        [HarmonyPatch(new Type[] { typeof(PawnGenerationRequest)}, new ArgumentType[] { ArgumentType.Normal })]
        public class GeneratePawn_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref Pawn __result)
            {
                try
                {
                    // All androids that need cores (by def extension) receive one by default. Cases where that is not desired can remove it there.
                    if (Utils.IsConsideredMechanicalAndroid(__result) && __result.def.GetModExtension<ATR_MechTweaker>()?.needsCoreAsAndroid == true)
                    {
                        __result.health.AddHediff(ATR_HediffDefOf.ATR_AutonomousCore, __result.health.hediffSet.GetBrain());
                    }
                    // Drones have some special mechanics that need to be specifically handled.
                    else if (Utils.IsConsideredMechanicalDrone(__result))
                    {
                        Utils.ReconfigureDrone(__result);
                    }
                    // Special pawns (by default only T5's) have no ideos or clothing.
                    else if (Utils.HasSpecialStatus(__result))
                    {
                        __result.ideo = null;
                        __result.apparel.DestroyAll();
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("[ATR] PawnGenerator.GeneratePawn " + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}