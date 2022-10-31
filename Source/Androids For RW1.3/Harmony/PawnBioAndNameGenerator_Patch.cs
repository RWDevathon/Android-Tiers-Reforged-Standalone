using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    if (Utils.HasSpecialStatus(pawn))
                        return;

                    switch (pawn.gender)
                    {
                        case Gender.Male:
                            __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, RulePackDefOf.ATR_AndroidMaleNames, pawn.story, RulePackDefOf.ATR_AndroidMaleNames, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);
                            break;
                        case Gender.Female:
                            __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, RulePackDefOf.ATR_AndroidFemaleNames, pawn.story, RulePackDefOf.ATR_AndroidFemaleNames, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);
                            break;
                        default:
                            __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, RulePackDefOf.ATR_AndroidNoneNames, pawn.story, RulePackDefOf.ATR_AndroidNoneNames, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);
                            break;
                    }
                    return;
                }
                // Mechanical drones never have gender. Generate a new name with the None name maker, ignoring xml tags.
                else if (Utils.IsConsideredMechanicalDrone(pawn))
                {
                    __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, RulePackDefOf.ATR_AndroidNoneNames, pawn.story, null, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);
                }
            }
        }
    }
}