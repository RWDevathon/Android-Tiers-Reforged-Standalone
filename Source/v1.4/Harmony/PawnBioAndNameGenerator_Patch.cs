using HarmonyLib;
using RimWorld;
using Verse;

namespace ATReforged
{
    internal class PawnBioAndNameGenerator_Patch
    {
        // Override the generation of names for Mechanical Androids and Drones. Because mod settings allow for swapping these things around, xml doesn't do the trick. This is Run-Time information.
        [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GeneratePawnName")]
        public class GeneratePawnName_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Name __result, NameStyle style = NameStyle.Full, string forcedLastName = null)
            {
                // Only override non-numeric name generations.
                if (style == NameStyle.Numeric)
                    return;

                // Mechanical androids may be Male, Female, or None based on settings. Handle name generation appropriately based on those settings.
                if (Utils.IsConsideredMechanicalAndroid(pawn))
                {
                    // Some special pawns should ignore unique name generation.
                    if (Utils.HasSpecialStatus(pawn))
                        return;

                    switch (pawn.gender)
                    {
                        // Androids that are male or female will try to use their xml name maker. Vanilla will handle the name scheme in this case.
                        case Gender.Male:
                        case Gender.Female:
                            break;
                        // Vanilla does not allow for None genders to have their own name maker, so if a race has genders but the pawn does not (IE. not all pawns of the race are genderless), we provide our own.
                        default:
                            if (pawn.RaceProps.hasGenders)
                            {
                                __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, ATR_RulePackDefOf.ATR_AndroidNoneNames, pawn.story, null, ATR_RulePackDefOf.ATR_AndroidNoneNames, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);
                            }
                            break;
                    }
                    return;
                }
                // Mechanical drones never have gender. Generate a new name with the None name maker, ignoring xml tags.
                else if (Utils.IsConsideredMechanicalDrone(pawn) && pawn.def.GetModExtension<ATR_MechTweaker>()?.letPawnKindHandleDroneBackstories == false)
                {
                    __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, ATR_RulePackDefOf.ATR_DroneNoneNames, pawn.story, null, null, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);
                    
                }
            }
        }
    }
}