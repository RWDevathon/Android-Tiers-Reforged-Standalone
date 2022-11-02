using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI;
using System.Linq;

using static ATReforged.Enums;
using System.Runtime;
using AlienRace;

namespace ATReforged
{
    public class ATReforged_Settings : ModSettings
    {
        // GENERAL SETTINGS
            // Settings for android gender
        public static bool androidsHaveGenders = false;
        public static bool androidsPickGenders = false;
        public static Gender androidsFixedGender = Gender.None;
        public static float androidsGenderRatio = 0.5f;

            // Settings for android commonality
        public static bool androidsAppearNaturally = true; // Androids can be random pawns
        public static bool androidSocietiesExist = true; // Android Factions allowed
        public static bool androidRaidersExist = true; // Android "Pirate" faction exists
        public static bool androidUnionistsExist = true; // Android "Civil Outlander" faction exists

            // Settings for Permissions
        public static HashSet<string> thingsAllowedAsRepairStims = new HashSet<string> { };
        public static HashSet<string> blacklistedMechanicalHediffs = new HashSet<string> { "ZeroGSickness", "SpaceHypoxia", "ClinicalDeathAsphyxiation", "ClinicalDeathNoHeartbeat", "FatalRad", "RimatomicsRadiation", "RadiationIncurable" };
        public static HashSet<string> blacklistedMechanicalTraits = new HashSet<string> { "NightOwl", "Insomniac", "Codependent", "HeavySleeper", "Polygamous", "Beauty", "Immunity" };

            // Settings for debug displays
        public static bool showMechanicalSurgerySuccessChance = true;
        
            // Settings for what is considered mechanical and massive
        public static HashSet<string> isConsideredMechanicalAnimal = new HashSet<string> { "DroneMineralUnit", "DroneNutritionUnit", "DroneChemUnit", "DroneWatchdog", "DroneTORT", "MicroScyther" };
        public static HashSet<string> isConsideredMechanicalAndroid = new HashSet<string> { "Tier2Android", "Tier3Android", "Tier4Android", "Tier5Android" };
        public static HashSet<string> isConsideredMechanicalDrone = new HashSet<string> { "Tier1Android", "M7Mech", "M8Mech" };
        public static HashSet<string> isConsideredMechanical = new HashSet<string> { "Tier1Android", "Tier2Android", "Tier3Android", "Tier4Android", "Tier5Android", "M7Mech", "M8Mech", "DroneMineralUnit", "DroneChemUnit", "DroneNutritionUnit", "DroneTORT", "DroneWatchdog" };
        public static HashSet<string> hasSpecialStatus = new HashSet<string> { };
        
            // Settings for what needs mechanical androids have
        public static bool androidsHaveJoyNeed = true;
        public static bool androidsHaveBeautyNeed = true;
        public static bool androidsHaveComfortNeed = false;
        public static bool androidsHaveOutdoorsNeed = false;

        // POWER SETTINGS
        public static int wattsConsumedPerBodySize = 500;
        public static bool useBatteryByDefault = false;
        public static bool mechanicalsHaveDifferentBioprocessingEfficiency = true;
        public static float mechanicalBioprocessingEfficiency = 0.5f;
        public static float batteryPercentagePerRareTick = 0.07f;

        public static HashSet<string> canUseBattery = new HashSet<string> { "Tier1Android", "Tier2Android", "Tier3Android", "Tier4Android", "Tier5Android", "M7Mech", "M8Mech", "DroneTORT", "DroneWatchdog" };

        // SECURITY SETTINGS
            // Settings for Enemy hacks
        public static bool enemyHacksOccur = true;
        public static float chanceAlliesInterceptHack = 0.05f;
        public static float pointsGainedOnInterceptPercentage = 0.25f;
        public static float enemyHackAttackStrengthModifier = 1.0f;
        public static float percentageOfValueUsedForRansoms = 0.25f;

            // Settings for player hacks
        public static bool playerCanHack = true;
        public static float chanceEnemiesInterceptHack = 0.4f;

        // HEALTH SETTINGS
            // Settings for Rust and maintenance
        public static bool mechanicalsCanRust = true;
        public static bool autoScheduleMechanicalMaintenance = true;
        public static bool autoSchedulePrisonerMechanicalMaintenance = true;
        public static int mechanicalRustMinDays = 35;
        public static int mechanicalRustMaxDays = 90;
        public static float randomMechanicalPaintOrRustChance = 0.15f;

            // Settings for Rebooting
        public static bool downedMechanicalsMustReboot = true;
        public static bool mechanicalsRebootAfterLowPower = true;
        public static float mechanicalRebootMinHours = 0.5f;
        public static float mechanicalRebootMaxHours = 1.5f;

            // Settings for Surgeries
        public static bool medicinesAreInterchangeable = false;
        public static float maxChanceSurgerySuccess = 1.0f;
        public static float chanceFailedSurgeryMinor = 0.75f;
        public static float chancePartSavedOnFailure = 0.75f;

        // CONNECTIVITY SETTINGS
            // Settings for Surrogates
        public static bool surrogatesAllowed = true;
        public static bool otherFactionsAllowedSurrogates = true;
        public static int minGroupSizeForSurrogates = 5;
        public static float minSurrogatePercentagePerLegalGroup = 0.2f;
        public static float maxSurrogatePercentagePerLegalGroup = 0.7f;
        
            // Settings for Cloud
        public static bool uploadingToSkyMindKills = true;
        public static bool uploadingToSkyMindPermaKills = true;
        public static int timeToCompleteSkyMindOperations = 12;
        public static int nbMoodPerAssistingMinds = 1;
        public static HashSet<string> pawnCanUseSkyMind = new HashSet<string> { "Tier1Android", "Tier2Android", "Tier3Android", "Tier4Android", "Tier5Android", "M8Mech" };
        public static HashSet<string> factionsUsingSkyMind = new HashSet<string> { "AndroidUnion", "MechanicalMarauders" };

            // Settings for Skill Points
        public static int skillPointConversionRate = 10;
        public static int passionSoftCap = 8;
        public static float basePointsNeededForPassion = 1000f;

        // STATS SETTINGS
            // Settings for Servers
        public static bool disableServersAmbiance = false;

        // INTERNAL SETTINGS
            // Settings page
        public OptionsTab activeTab = OptionsTab.General;
        public SettingsPreset ActivePreset = SettingsPreset.None;
        public bool settingsEverOpened = false;

        public void StartupChecks()
        {
            if (ActivePreset == SettingsPreset.None)
            {
                settingsEverOpened = false;
                ApplyPreset(SettingsPreset.Default);
            }
        }

        Vector2 scrollPosition = Vector2.zero;
        float cachedScrollHeight = 0;
        internal void DoSettingsWindowContents(Rect inRect)
        {
            settingsEverOpened = true;
            bool hasChanged = false;
            void onChange() { hasChanged = true; }

            Color colorSave = GUI.color;
            TextAnchor anchorSave = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            var headerRect = inRect.TopPartPixels(50);
            var restOfRect = new Rect(inRect);
            restOfRect.y += 50;
            restOfRect.height -= 50;

            Listing_Standard prelist = new Listing_Standard();
            prelist.Begin(headerRect);

            prelist.EnumSelector("ATR_SettingsTabTitle".Translate(), ref activeTab, "ATR_SettingsTabOption_", valueTooltipPostfix: null);
            prelist.GapLine();

            prelist.End();

            bool needToScroll = cachedScrollHeight > inRect.height;
            var viewRect = new Rect(restOfRect);
            if (needToScroll)
            {
                viewRect.width -= 20f;
                viewRect.height = cachedScrollHeight;
                Widgets.BeginScrollView(restOfRect, ref scrollPosition, viewRect);
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            listingStandard.Begin(viewRect);

            // Add switch for which tab you are on here
            switch (activeTab)
            {
                case OptionsTab.General:
                {
                    // PRESET SETTINGS
                    if (listingStandard.ButtonText("ATR_ApplyPreset".Translate()))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (SettingsPreset s in Enum.GetValues(typeof(SettingsPreset)))
                        {
                            if (s == SettingsPreset.None) // Can not apply the None preset.
                            {
                                continue;
                            }
                            options.Add(new FloatMenuOption(("ATR_SettingsPreset" + s.ToString()).Translate(), () => ApplyPreset(s)));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    listingStandard.GapLine();

                    // GENDER SETTINGS
                    listingStandard.CheckboxLabeled("ATR_AndroidsHaveGenders".Translate(), ref androidsHaveGenders, tooltip:"ATR_AndroidGenderNotice".Translate(), onChange: onChange);

                    if (androidsHaveGenders)
                    {
                        listingStandard.CheckboxLabeled("ATR_AndroidsPickGenders".Translate(), ref androidsPickGenders, tooltip: "ATR_AndroidGenderNotice".Translate(), onChange: onChange);
                    }

                    if (androidsHaveGenders && !androidsPickGenders)
                    { // If Androids have genders but don't pick them, players must choose which gender they all are. 0 = Male, 1 = Female
                        bool fixedGender = androidsFixedGender == Gender.Female;
                        listingStandard.CheckboxLabeled("ATR_AndroidsFixedGenderSelector".Translate(), ref fixedGender, tooltip: "ATR_AndroidGenderNotice".Translate(), onChange: onChange);
                        androidsFixedGender = fixedGender ? Gender.Female: Gender.Male;
                    }

                    if (androidsHaveGenders && androidsPickGenders)
                    { // If Androids have genders and pick, players can choose how often they pick male or female.
                        listingStandard.SliderLabeled("ATR_AndroidsGenderRatio".Translate(), ref androidsGenderRatio, 0.0f, 1.0f, displayMult: 100, onChange: onChange);
                    }
                    listingStandard.GapLine();

                    // COMMONALITY SETTINGS
                    listingStandard.CheckboxLabeled("ATR_AndroidsAppearNaturally".Translate(), ref androidsAppearNaturally, tooltip: "ATR_PlayerOnlyNote".Translate(), onChange: onChange);
                    if (androidsAppearNaturally)
                    {
                        listingStandard.CheckboxLabeled("ATR_AndroidSocietiesExist".Translate(), ref androidSocietiesExist, onChange: onChange);
                        if (androidSocietiesExist)
                        {
                            listingStandard.CheckboxLabeled("ATR_AndroidRaidersExist".Translate(), ref androidRaidersExist, tooltip: "ATR_AndroidRaidersDesc".Translate(), onChange: onChange);
                            listingStandard.CheckboxLabeled("ATR_AndroidUnionistsExist".Translate(), ref androidUnionistsExist, tooltip: "ATR_AndroidUnionistsDesc".Translate(), onChange: onChange);
                        }
                    }
                    listingStandard.GapLine();

                    // CONSIDERATION SETTINGS
                    
                    // NEEDS SETTINGS
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedJoy".Translate(), ref androidsHaveJoyNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedBeauty".Translate(), ref androidsHaveBeautyNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedComfort".Translate(), ref androidsHaveComfortNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedOutdoors".Translate(), ref androidsHaveOutdoorsNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    break;
                }
                case OptionsTab.Power:
                {
                    listingStandard.CheckboxLabeled("ATR_useBatteryByDefault".Translate(), ref useBatteryByDefault, onChange: onChange);
                    string wattsConsumedBuffer = wattsConsumedPerBodySize.ToString();
                    listingStandard.TextFieldNumericLabeled("ATR_wattsConsumedPerBodySize".Translate(), ref wattsConsumedPerBodySize, ref wattsConsumedBuffer, 0, 2000);
                    listingStandard.SliderLabeled("ATR_batteryPercentagePerRareTick".Translate(), ref batteryPercentagePerRareTick, 0.01f, 1f, displayMult: 100, onChange: onChange);

                    listingStandard.GapLine();

                    listingStandard.CheckboxLabeled("ATR_mechanicalsHaveDifferentBioprocessingEfficiency".Translate(), ref mechanicalsHaveDifferentBioprocessingEfficiency, onChange: onChange);
                    if (mechanicalsHaveDifferentBioprocessingEfficiency)
                        {
                        listingStandard.SliderLabeled("ATR_mechanicalBioprocessingEfficiency".Translate(), ref mechanicalBioprocessingEfficiency, 0.1f, 2.0f, displayMult: 100, onChange: onChange);
                    }
                    break;
                }
                case OptionsTab.Connectivity:
                    {
                        string skillPointConversionRateBuffer = skillPointConversionRate.ToString();
                        string passionSoftCapBuffer = passionSoftCap.ToString();
                        string basePointsNeededForPassionBuffer = basePointsNeededForPassion.ToString();
                        listingStandard.TextFieldNumericLabeled("ATR_skillPointConversionRate".Translate(), ref skillPointConversionRate, ref skillPointConversionRateBuffer, 1, 500);
                        listingStandard.TextFieldNumericLabeled("ATR_passionSoftCap".Translate(), ref passionSoftCap, ref passionSoftCapBuffer, 0, 50);
                        listingStandard.TextFieldNumericLabeled("ATR_basePointsNeededForPassion".Translate(), ref basePointsNeededForPassion, ref basePointsNeededForPassionBuffer, 10, 10000);
                        break;
                    }
                default:
                {
                    break;
                }
            }
            // Ending

            cachedScrollHeight = listingStandard.CurHeight;
            listingStandard.End();

            if (needToScroll)
            {
                Widgets.EndScrollView();
            }


            if (hasChanged)
                ApplyPreset(SettingsPreset.Custom);

            GUI.color = colorSave;
            Text.Anchor = anchorSave;
        }
        
        public void ApplyBaseSettings()
        {
            // Reset Gender Settings
            androidsHaveGenders = false;
            androidsPickGenders = false;
            androidsFixedGender = 0;
            androidsGenderRatio = 0.5f;

            // Android Commonality Settings
            androidsAppearNaturally = true;
            androidSocietiesExist = true;
            androidRaidersExist = true;
            androidUnionistsExist = true;

            // Permissions
            thingsAllowedAsRepairStims = new HashSet<string> { };
            blacklistedMechanicalHediffs = new HashSet<string> { "ZeroGSickness", "SpaceHypoxia", "ClinicalDeathAsphyxiation", "ClinicalDeathNoHeartbeat", "FatalRad", "RimatomicsRadiation", "RadiationIncurable" };
            blacklistedMechanicalTraits = new HashSet<string> { "NightOwl", "Insomniac", "Codependent", "HeavySleeper", "Polygamous", "Beauty", "Immunity" };

            // Consideration Settings
            isConsideredMechanicalAndroid = new HashSet<string> { "Tier2Android", "Tier3Android", "Tier4Android", "Tier5Android" };
            isConsideredMechanicalDrone = new HashSet<string> { "Tier1Android", "M7Mech", "M8Mech" };
            isConsideredMechanicalAnimal = new HashSet<string> { "DroneMineralUnit", "DroneNutritionUnit", "DroneChemUnit", "DroneWatchdog", "DroneTORT", "MicroScyther" };
            isConsideredMechanical = new HashSet<string> { "Tier1Android", "Tier2Android", "Tier3Android", "Tier4Android", "Tier5Android", "M7Mech", "M8Mech", "DroneMineralUnit", "DroneChemUnit", "DroneNutritionUnit", "DroneTORT", "DroneWatchdog", "MicroScyther" };
            hasSpecialStatus = new HashSet<string> { };

            // Needs Settings
            androidsHaveJoyNeed = true;
            androidsHaveBeautyNeed = true;
            androidsHaveComfortNeed = false;
            androidsHaveOutdoorsNeed = false;

            // POWER SETTINGS
            wattsConsumedPerBodySize = 500;
            useBatteryByDefault = false;
            mechanicalsHaveDifferentBioprocessingEfficiency = true;
            mechanicalBioprocessingEfficiency = 0.5f;
            batteryPercentagePerRareTick = 0.07f;


            // CONNECTIVITY SETTINGS
            skillPointConversionRate = 10;
            passionSoftCap = 8;
            basePointsNeededForPassion = 1000f;
    }

        public void ApplyPreset(SettingsPreset preset)
        {
            if (preset == SettingsPreset.None)
                throw new InvalidOperationException("[ATR] Applying the None preset is illegal - were the mod options properly initialized?");

            ActivePreset = preset;
            if (preset == SettingsPreset.Custom) // Custom settings are inherently not a preset, so apply no new settings.
                return;

            ApplyBaseSettings();

            switch (preset)
            {
                case SettingsPreset.Default:
                    break;
                default:
                    throw new InvalidOperationException("Attempted to apply a nonexistent preset.");
            }

            //RebuildCache(ref LimitModeSingle_Match_Cache, PawnListKind.Android);
            //RebuildCache(ref LimitModeSingleMelee_Match_Cache, PawnListKind.Drone);
            //RebuildCache(ref LimitModeSingleRanged_Match_Cache, PawnListKind.Animal);
        }

        private void RebuildCache(ref HashSet<ThingDef> cache, PawnListKind listType)
        {
            IEnumerable<ThingDef> validPawns = FilteredGetters.getValidPawns();

            //Log.Message($"(list type: {listType}) valid weapons ({validSidearms.Count()}):{String.Join(", ", validSidearms.Select(w => w.defName))}");

            IEnumerable<ThingDef> matchingPawns = FilteredGetters.FilterByPawnKind(validPawns, listType);

            //Log.Message($"candidate weapons ({matchingSidearms.Count()}):{String.Join(", ", matchingSidearms.Select(w => w.defName))}");

            switch (listType)
            {
                case PawnListKind.Android:
                    break;
                case PawnListKind.Drone:
                    break;
                case PawnListKind.Animal:
                    break;
                default:
                    throw new ArgumentException();
            }

            //Log.Message($"(result weapons ({matchingSidearms.Count()}):{String.Join(", ", matchingSidearms.Select(w => w.defName))}");

            cache = matchingPawns.ToHashSet();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            /* == INTERNAL === */
            Scribe_Values.Look(ref ActivePreset, "ATR_ActivePreset", SettingsPreset.None, true);

            /* === GENERAL === */

            // Gender
            Scribe_Values.Look(ref androidsHaveGenders, "ATR_androidsHaveGenders", false);
            Scribe_Values.Look(ref androidsPickGenders, "ATR_androidsPickGenders", false);
            Scribe_Values.Look(ref androidsFixedGender, "ATR_androidsFixedGender", Gender.None);
            Scribe_Values.Look(ref androidsGenderRatio, "ATR_androidsGenderRatio", 0.5f);

            // Commonality
            Scribe_Values.Look(ref androidsAppearNaturally, "ATR_androidsAppearNaturally", true);
            Scribe_Values.Look(ref androidSocietiesExist, "ATR_androidSocietiesExist", true);
            Scribe_Values.Look(ref androidRaidersExist, "ATR_androidRaidersExist", true);
            Scribe_Values.Look(ref androidUnionistsExist, "ATR_androidUnionistsExist", true);

            // Permissions
            Scribe_Collections.Look(ref thingsAllowedAsRepairStims, "ATR_thingsAllowedAsRepairStims", LookMode.Value);
            Scribe_Collections.Look(ref blacklistedMechanicalHediffs, "ATR_blacklistedMechanicalHediffs", LookMode.Value);
            Scribe_Collections.Look(ref blacklistedMechanicalTraits, "ATR_blacklistedMechanicalTraits", LookMode.Value);

            // Considerations
            Scribe_Collections.Look(ref isConsideredMechanicalAnimal, "ATR_isConsideredMechanicalAnimal", LookMode.Value);
            Scribe_Collections.Look(ref isConsideredMechanicalAndroid, "ATR_isConsideredMechanicalAndroid", LookMode.Value);
            Scribe_Collections.Look(ref isConsideredMechanicalDrone, "ATR_isConsideredMechanicalDrone", LookMode.Value);
            Scribe_Collections.Look(ref isConsideredMechanical, "ATR_isConsideredMechanical", LookMode.Value);
            Scribe_Collections.Look(ref hasSpecialStatus, "ATR_hasSpecialStatus", LookMode.Value);

            // Needs
            Scribe_Values.Look(ref androidsHaveJoyNeed, "ATR_androidsHaveJoyNeed", true);
            Scribe_Values.Look(ref androidsHaveBeautyNeed, "ATR_androidsHaveBeautyNeed", true);
            Scribe_Values.Look(ref androidsHaveComfortNeed, "ATR_androidsHaveComfortNeed", false);
            Scribe_Values.Look(ref androidsHaveOutdoorsNeed, "ATR_androidsHaveOutdoorsNeed", false);

            /* === POWER === */

            Scribe_Values.Look(ref wattsConsumedPerBodySize, "ATR_wattsConsumedPerBodySize", 500);
            Scribe_Values.Look(ref useBatteryByDefault, "ATR_useBatteryByDefault", false);
            Scribe_Values.Look(ref mechanicalsHaveDifferentBioprocessingEfficiency, "ATR_mechanicalsHaveDifferentBioprocessingEfficiency", true);
            Scribe_Values.Look(ref mechanicalBioprocessingEfficiency, "ATR_mechanicalBioprocessingEfficiency", 0.5f);
            Scribe_Values.Look(ref batteryPercentagePerRareTick, "ATR_batteryPercentagePerRareTick", 0.07f);

            /* === CONNECTIVITY === */

            // Skills
            Scribe_Values.Look(ref skillPointConversionRate, "ATR_skillPointConversionRate", 10);
            Scribe_Values.Look(ref passionSoftCap, "ATR_passionSoftCap", 8);
            Scribe_Values.Look(ref basePointsNeededForPassion, "ATR_basePointsNeededForPassion", 1000f);
        }
    }

}