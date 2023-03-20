using Verse;
using HarmonyLib;
using AlienRace;
using RimWorld;

namespace ATReforged
{
    // NOTE: This is for harmony patches on Humanoid Alien Race's Assembly, not Core!
    // Piggyback off HAR's code to make sure androids do not end up with blacklisted traits.
    public class RaceRestrictionSettings_Patch
    {
        [HarmonyPatch(typeof(RaceRestrictionSettings), "CanGetTrait")]
        public class CanGetTrait_Patch
        {
            [HarmonyPostfix]
            public static void Listener(TraitDef trait, ThingDef race, int degree, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                // If HAR's race settings whitelists this trait, yield to that setting.
                RaceRestrictionSettings raceRestrictionSettings = (race as ThingDef_AlienRace)?.alienRace?.raceRestriction;
                if (raceRestrictionSettings?.whiteTraitList.Contains(trait) == true)
                {
                    return;
                }

                // If the pawn is an android and this trait is blacklisted, it can not have it.
                if (Utils.IsConsideredMechanicalAndroid(race) && ATReforged_Settings.blacklistedMechanicalTraits.Contains(trait.defName))
                {
                    __result = false;
                }
            }
        }
    }
}