using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class Pawn_NeedsTracker_Patch
    {
        // Ensure mechanical units only have applicable needs as determined by settings.
        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
        public class ShouldHaveNeed_Patch
        {
            [HarmonyPostfix]
            public static void Listener(NeedDef nd, ref bool __result, Pawn ___pawn)
            {
                // Patch only applies to mechanical units.
                if (!__result || ___pawn == null || !Utils.IsConsideredMechanical(___pawn))
                    return;

                switch (nd.defName)
                {
                    case "Mood":
                        __result = !Utils.IsConsideredMechanicalDrone(___pawn) && !Utils.HasSpecialStatus(___pawn);
                        break;
                    case "Joy":
                        __result = !Utils.IsConsideredMechanicalDrone(___pawn) && !Utils.HasSpecialStatus(___pawn) && ATReforged_Settings.androidsHaveJoyNeed;
                        return;
                    case "Beauty":
                        __result = !Utils.IsConsideredMechanicalDrone(___pawn) && !Utils.HasSpecialStatus(___pawn) && ATReforged_Settings.androidsHaveBeautyNeed;
                        return;
                    case "Outdoors":
                        __result = !Utils.IsConsideredMechanicalDrone(___pawn) && !Utils.HasSpecialStatus(___pawn) && ATReforged_Settings.androidsHaveOutdoorsNeed;
                        return;
                    case "Indoors":
                        __result = !Utils.IsConsideredMechanicalDrone(___pawn) && !Utils.HasSpecialStatus(___pawn) && ATReforged_Settings.androidsHaveOutdoorsNeed;
                        return;
                    case "Comfort":
                        __result = !Utils.IsConsideredMechanicalDrone(___pawn) && !Utils.HasSpecialStatus(___pawn) && ATReforged_Settings.androidsHaveComfortNeed;
                        return;
                    case "Hygiene":
                    case "Bladder":
                    case "DBHThirst":
                        __result = false;
                        return;
                }
            }
        }
    }
}