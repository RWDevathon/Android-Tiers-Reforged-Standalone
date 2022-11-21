using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

using static ATReforged.Enums;

namespace ATReforged
{
    public class ATReforged_Settings : ModSettings
    {
        // GENERAL SETTINGS
            // Settings for android gender
        public static bool androidsHaveGenders;
        public static bool androidsPickGenders;
        public static Gender androidsFixedGender;
        public static float androidsGenderRatio;

            // Settings for Permissions
        public static HashSet<string> thingsAllowedAsRepairStims = new HashSet<string> { };
        public static HashSet<string> blacklistedMechanicalHediffs = new HashSet<string> { "ZeroGSickness", "SpaceHypoxia", "ClinicalDeathAsphyxiation", "ClinicalDeathNoHeartbeat", "FatalRad", "RimatomicsRadiation", "RadiationIncurable" };
        public static HashSet<string> blacklistedMechanicalTraits = new HashSet<string> { "NightOwl", "Insomniac", "Codependent", "HeavySleeper", "Polygamous", "Beauty", "Immunity" };

            // Settings for debug displays
        public static bool showMechanicalSurgerySuccessChance = true;
        
            // Settings for what is considered mechanical and massive
        public static HashSet<ThingDef> isConsideredMechanicalAnimal;
        public static HashSet<ThingDef> isConsideredMechanicalAndroid;
        public static HashSet<ThingDef> isConsideredMechanicalDrone;
        public static HashSet<ThingDef> isConsideredMechanical;
        public static HashSet<ThingDef> hasSpecialStatus;
        
            // Settings for what needs mechanical androids have
        public static bool androidsHaveJoyNeed;
        public static bool androidsHaveBeautyNeed;
        public static bool androidsHaveComfortNeed;
        public static bool androidsHaveOutdoorsNeed;

        // POWER SETTINGS
        public static int wattsConsumedPerBodySize;
        public static bool chargeCapableMeansDifferentBioEfficiency;
        public static float chargeCapableBioEfficiency;
        public static float batteryChargeRate;

        public static HashSet<ThingDef> canUseBattery;

        // SECURITY SETTINGS
            // Settings for Enemy hacks
        public static bool enemyHacksOccur;
        public static float chanceAlliesInterceptHack;
        public static float pointsGainedOnInterceptPercentage;
        public static float enemyHackAttackStrengthModifier;
        public static float percentageOfValueUsedForRansoms;

            // Settings for player hacks
        public static bool playerCanHack = true;
        public static bool receiveHackingAlert = true;
        public static float retaliationChanceOnFailure = 0.4f;
        public static float minHackSuccessChance = 0.05f;
        public static float maxHackSuccessChance = 0.95f;

        // HEALTH SETTINGS

            // Settings for Surgeries
        public static bool medicinesAreInterchangeable = false;
        public static float maxChanceMechanicOperationSuccess = 1.0f;
        public static float chanceFailedOperationMinor = 0.75f;
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
        public static HashSet<string> factionsUsingSkyMind = new HashSet<string> { "AndroidUnion", "MechanicalMarauders" };

            // Settings for Skill Points
        public static bool receiveSkillAlert = true;
        public static int skillPointConversionRate = 10;
        public static int passionSoftCap = 8;
        public static float basePointsNeededForPassion = 1000f;

        // STATS SETTINGS

        // INTERNAL SETTINGS
            // Settings page
        public OptionsTab activeTab = OptionsTab.General;
        public SettingsPreset ActivePreset = SettingsPreset.None;
        public bool settingsEverOpened = false;

        public void StartupChecks()
        {
            if (isConsideredMechanicalAndroid == null)
                isConsideredMechanicalAndroid = new HashSet<ThingDef>();
            if (isConsideredMechanicalDrone == null)
                isConsideredMechanicalDrone = new HashSet<ThingDef>();
            if (isConsideredMechanicalAnimal == null)
                isConsideredMechanicalAnimal = new HashSet<ThingDef>();
            if (isConsideredMechanical == null)
                isConsideredMechanical = new HashSet<ThingDef>();
            if (hasSpecialStatus == null)
                hasSpecialStatus = new HashSet<ThingDef>();
            if (canUseBattery == null)
                canUseBattery = new HashSet<ThingDef>();
            if (ActivePreset == SettingsPreset.None)
            {
                settingsEverOpened = false;
                ApplyPreset(SettingsPreset.Default);
            }
        }

        Vector2 scrollPosition = Vector2.zero;
        float cachedScrollHeight = 0;

        bool cachedExpandFirst = true;
        bool cachedExpandSecond = true;
        bool cachedExpandThird = true;

        void ResetCachedExpand() 
        { 
            cachedExpandFirst = true; 
            cachedExpandSecond = true; 
            cachedExpandThird = true;
        }

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

            prelist.EnumSelector("ATR_SettingsTabTitle".Translate(), ref activeTab, "ATR_SettingsTabOption_", valueTooltipPostfix: null, onChange: ResetCachedExpand);
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

            Listing_Standard listingStandard = new Listing_Standard
            {
                maxOneColumn = true
            };
            listingStandard.Begin(viewRect);

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

                    // CONSIDERATION SETTINGS
                    listingStandard.Label("ATR_RestartRequiredSectionDesc".Translate());
                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                            cachedExpandFirst = !cachedExpandFirst;
                    }
                    if (cachedExpandFirst)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Humanlike), isConsideredMechanicalAndroid, "ATR_SettingsConsideredAndroid".Translate(), "ATR_SettingsNotConsideredAndroid".Translate(), onChange);
                    
                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                        cachedExpandSecond = !cachedExpandSecond;
                    }
                    if (cachedExpandSecond)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Humanlike), isConsideredMechanicalDrone, "ATR_SettingsConsideredDrone".Translate(), "ATR_SettingsNotConsideredDrone".Translate(), onChange);
                    
                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                        cachedExpandThird = !cachedExpandThird;
                    }
                    if (cachedExpandThird)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Animal), isConsideredMechanicalAnimal, "ATR_SettingsConsideredAnimal".Translate(), "ATR_SettingsNotConsideredAnimals".Translate(), onChange);
                    
                    listingStandard.GapLine();

                    // NEEDS SETTINGS
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedJoy".Translate(), ref androidsHaveJoyNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedBeauty".Translate(), ref androidsHaveBeautyNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedComfort".Translate(), ref androidsHaveComfortNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_AndroidsNeedOutdoors".Translate(), ref androidsHaveOutdoorsNeed, tooltip: "ATR_AndroidOnlyNotice".Translate(), onChange: onChange);
                    break;
                }
                case OptionsTab.Power:
                {
                    listingStandard.SliderLabeled("ATR_batteryPercentagePerTick".Translate(), ref batteryChargeRate, 0.1f, 4f, onChange: onChange);

                    listingStandard.GapLine();

                    listingStandard.CheckboxLabeled("ATR_mechanicalsHaveDifferentBioprocessingEfficiency".Translate(), ref chargeCapableMeansDifferentBioEfficiency, onChange: onChange);
                    if (chargeCapableMeansDifferentBioEfficiency)
                        {
                        listingStandard.SliderLabeled("ATR_mechanicalBioprocessingEfficiency".Translate(), ref chargeCapableBioEfficiency, 0.1f, 2.0f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    }
                    break;
                }
                case OptionsTab.Security:
                {
                    listingStandard.CheckboxLabeled("ATR_EnemyHacksOccur".Translate(), ref enemyHacksOccur, onChange: onChange);
                    if (enemyHacksOccur)
                    {
                        listingStandard.SliderLabeled("ATR_EnemyHackAttackStrengthModifier".Translate(), ref enemyHackAttackStrengthModifier, 0.01f, 5f, displayMult: 100, valueSuffix: "%", tooltip: "ATR_EnemyHackAttackStrengthModifierDesc".Translate(), onChange: onChange);
                        listingStandard.SliderLabeled("ATR_ChanceAlliesInterceptHack".Translate(), ref chanceAlliesInterceptHack, 0.01f, 1f, displayMult: 100, valueSuffix: "%", tooltip: "ATR_ChanceAlliesInterceptHackDesc".Translate(), onChange: onChange);
                        listingStandard.SliderLabeled("ATR_PointsGainedOnInterceptPercentage".Translate(), ref pointsGainedOnInterceptPercentage, 0.00f, 3f, displayMult: 100, valueSuffix: "%", tooltip: "ATR_PointsGainedOnInterceptPercentageDesc".Translate(), onChange: onChange);
                        listingStandard.SliderLabeled("ATR_PercentageOfValueUsedForRansoms".Translate(), ref percentageOfValueUsedForRansoms, 0.01f, 2f, displayMult: 100, valueSuffix:"%", onChange: onChange);
                    }



                    listingStandard.CheckboxLabeled("ATR_PlayerCanHack".Translate(), ref playerCanHack, onChange: onChange);
                    if (playerCanHack)
                    {
                        listingStandard.CheckboxLabeled("ATR_receiveFullHackingAlert".Translate(), ref receiveHackingAlert, onChange: onChange);
                        listingStandard.SliderLabeled("ATR_RetaliationChanceOnFailure".Translate(), ref retaliationChanceOnFailure, 0.0f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        listingStandard.SliderLabeled("ATR_MinHackSuccessChance".Translate(), ref minHackSuccessChance, 0.0f, maxHackSuccessChance, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        listingStandard.SliderLabeled("ATR_MaxHackSuccessChance".Translate(), ref maxHackSuccessChance, minHackSuccessChance, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    }
                    break;
                }
                case OptionsTab.Connectivity:
                {
                    string skillPointConversionRateBuffer = skillPointConversionRate.ToString();
                    string passionSoftCapBuffer = passionSoftCap.ToString();
                    string basePointsNeededForPassionBuffer = basePointsNeededForPassion.ToString();
                    listingStandard.CheckboxLabeled("ATR_receiveFullSkillAlert".Translate(), ref receiveSkillAlert, onChange: onChange);
                    listingStandard.TextFieldNumericLabeled("ATR_skillPointConversionRate".Translate(), ref skillPointConversionRate, ref skillPointConversionRateBuffer, 1, 500);
                    listingStandard.TextFieldNumericLabeled("ATR_passionSoftCap".Translate(), ref passionSoftCap, ref passionSoftCapBuffer, 0, 50);
                    listingStandard.TextFieldNumericLabeled("ATR_basePointsNeededForPassion".Translate(), ref basePointsNeededForPassion, ref basePointsNeededForPassionBuffer, 10, 10000);
                    listingStandard.GapLine();


                    listingStandard.CheckboxLabeled("ATR_UploadingKills".Translate(), ref uploadingToSkyMindKills, onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_UploadingPermakills".Translate(), ref uploadingToSkyMindPermaKills, onChange: onChange);
                    string SkyMindOperationTimeBuffer = timeToCompleteSkyMindOperations.ToString();
                    listingStandard.TextFieldNumericLabeled("ATR_SkyMindOperationTimeRequired".Translate(), ref timeToCompleteSkyMindOperations, ref SkyMindOperationTimeBuffer, 1, 50);
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

            // Permissions
            thingsAllowedAsRepairStims = new HashSet<string> { };
            blacklistedMechanicalHediffs = new HashSet<string> { "ZeroGSickness", "SpaceHypoxia", "ClinicalDeathAsphyxiation", "ClinicalDeathNoHeartbeat", "FatalRad", "RimatomicsRadiation", "RadiationIncurable" };
            blacklistedMechanicalTraits = new HashSet<string> { "NightOwl", "Insomniac", "Codependent", "HeavySleeper", "Polygamous", "Beauty", "Immunity" };

            // Needs Settings
            androidsHaveJoyNeed = true;
            androidsHaveBeautyNeed = true;
            androidsHaveComfortNeed = false;
            androidsHaveOutdoorsNeed = false;

            // POWER SETTINGS
            wattsConsumedPerBodySize = 500;
            chargeCapableMeansDifferentBioEfficiency = true;
            chargeCapableBioEfficiency = 0.5f;
            batteryChargeRate = 1f;

            // SECURITY SETTINGS
            enemyHacksOccur = true;
            chanceAlliesInterceptHack = 0.05f;
            pointsGainedOnInterceptPercentage = 0.25f;
            enemyHackAttackStrengthModifier = 1.0f;
            percentageOfValueUsedForRansoms = 0.25f;

            playerCanHack = true;
            receiveHackingAlert = true;
            retaliationChanceOnFailure = 0.4f;
            minHackSuccessChance = 0.05f;
            maxHackSuccessChance = 0.95f;

            // CONNECTIVITY SETTINGS
            // Skills
            skillPointConversionRate = 10;
            passionSoftCap = 8;
            basePointsNeededForPassion = 1000f;

            // Cloud
            receiveSkillAlert = true;
            uploadingToSkyMindKills = true;
            uploadingToSkyMindPermaKills = true;
            timeToCompleteSkyMindOperations = 12;

            RebuildCaches();
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

        }
        
        // Caches for ThingDefs must be rebuilt manually. Configuration uses the ATR_MechTweaker by default and will capture all pawn thing defs with that modExtension.
        private void RebuildCaches()
        {
            IEnumerable<ThingDef> validPawns = FilteredGetters.GetValidPawns();

            HashSet<ThingDef> matchingAndroids = new HashSet<ThingDef>();
            HashSet<ThingDef> matchingDrones = new HashSet<ThingDef>();
            HashSet<ThingDef> matchingMechanicals = new HashSet<ThingDef>();
            HashSet<ThingDef> matchingSpecials = new HashSet<ThingDef>();
            HashSet<ThingDef> matchingChargers = new HashSet<ThingDef>();
            foreach (ThingDef validHumanlike in FilteredGetters.FilterByIntelligence(validPawns, Intelligence.Humanlike).Where(thingDef => thingDef.HasModExtension<ATR_MechTweaker>()))
            {
                // Mechanical Androids are humanlikes with global learning factor >= 0.5 that have the ModExtension
                if (validHumanlike.statBases?.GetStatValueFromList(RimWorld.StatDefOf.GlobalLearningFactor, 0.5f) >= 0.5f)
                {
                    matchingAndroids.Add(validHumanlike);
                    // Particularly high global learning factor implies this unit is not a normal android but is a higher power.
                    if (validHumanlike.statBases?.GetStatValueFromList(RimWorld.StatDefOf.GlobalLearningFactor, 0.5f) >= 4f)
                        matchingSpecials.Add(validHumanlike);
                }
                // Mechanical Drones are humanlikes with global learning factor < 0.5 that have the ModExtension
                else
                {
                    matchingDrones.Add(validHumanlike);
                }
                // All mechanical humanlikes may charge inherently.
                matchingChargers.Add(validHumanlike);
                matchingMechanicals.Add(validHumanlike);
            }
            // Mechanical animals are animals that have the ModExtension
            HashSet<ThingDef> matchingAnimals = FilteredGetters.FilterByIntelligence(validPawns, Intelligence.Animal).Where(thingDef => thingDef.HasModExtension<ATR_MechTweaker>()).ToHashSet();

            // Mechanical animals of advanced intelligence may charge.
            foreach (ThingDef validAnimal in matchingAnimals)
            {
                matchingMechanicals.Add(validAnimal);
                // Advanced mechanical animals may charge.
                if (validAnimal.race.trainability == TrainabilityDefOf.Advanced)
                    matchingChargers.Add(validAnimal);
            }

            isConsideredMechanicalAndroid = matchingAndroids;
            isConsideredMechanicalDrone = matchingDrones;
            isConsideredMechanicalAnimal = matchingAnimals;
            isConsideredMechanical = matchingMechanicals;
            hasSpecialStatus = matchingSpecials;
            canUseBattery = matchingChargers;
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

            // Permissions
            Scribe_Collections.Look(ref thingsAllowedAsRepairStims, "ATR_thingsAllowedAsRepairStims", LookMode.Value);
            Scribe_Collections.Look(ref blacklistedMechanicalHediffs, "ATR_blacklistedMechanicalHediffs", LookMode.Value);
            Scribe_Collections.Look(ref blacklistedMechanicalTraits, "ATR_blacklistedMechanicalTraits", LookMode.Value);

            // Considerations
            try
            {
                Scribe_Collections.Look(ref isConsideredMechanicalAnimal, "ATR_isConsideredMechanicalAnimal", LookMode.Def);
                Scribe_Collections.Look(ref isConsideredMechanicalAndroid, "ATR_isConsideredMechanicalAndroid", LookMode.Def);
                Scribe_Collections.Look(ref isConsideredMechanicalDrone, "ATR_isConsideredMechanicalDrone", LookMode.Def);
                Scribe_Collections.Look(ref isConsideredMechanical, "ATR_isConsideredMechanical", LookMode.Def);
                Scribe_Collections.Look(ref hasSpecialStatus, "ATR_hasSpecialStatus", LookMode.Def);
            }
            catch (Exception ex)
            {
                Log.Warning("[ATR] Mod settings failed to load appropriately! Resetting to default to avoid further issues. " + ex.Message + " " + ex.StackTrace);
                ApplyPreset(SettingsPreset.Default);
            }

            // Needs
            Scribe_Values.Look(ref androidsHaveJoyNeed, "ATR_androidsHaveJoyNeed", true);
            Scribe_Values.Look(ref androidsHaveBeautyNeed, "ATR_androidsHaveBeautyNeed", true);
            Scribe_Values.Look(ref androidsHaveComfortNeed, "ATR_androidsHaveComfortNeed", false);
            Scribe_Values.Look(ref androidsHaveOutdoorsNeed, "ATR_androidsHaveOutdoorsNeed", false);

            /* === POWER === */

            Scribe_Values.Look(ref wattsConsumedPerBodySize, "ATR_wattsConsumedPerBodySize", 500);
            Scribe_Values.Look(ref chargeCapableMeansDifferentBioEfficiency, "ATR_chargeCapableMeansDifferentBioEfficiency", true);
            Scribe_Values.Look(ref chargeCapableBioEfficiency, "ATR_chargeCapableBioEfficiency", 0.5f);
            Scribe_Values.Look(ref batteryChargeRate, "ATR_batteryChargeRate", 1f);

            try
            {
                Scribe_Collections.Look(ref canUseBattery, "ATR_canUseBattery", LookMode.Def);
            }
            catch (Exception ex)
            {
                Log.Warning("[ATR] Mod settings failed to load appropriately! Resetting to default to avoid further issues. " + ex.Message + " " + ex.StackTrace);
                ApplyPreset(SettingsPreset.Default);
            }

            /* === SECURITY === */

            // Hostile Hacks
            Scribe_Values.Look(ref enemyHacksOccur, "ATR_enemyHacksOccur", true);
            Scribe_Values.Look(ref chanceAlliesInterceptHack, "ATR_chanceAlliesInterceptHack", 0.05f);
            Scribe_Values.Look(ref pointsGainedOnInterceptPercentage, "ATR_pointsGainedOnInterceptPercentage", 0.25f);
            Scribe_Values.Look(ref enemyHackAttackStrengthModifier, "ATR_enemyHackAttackStrengthModifier", 1.0f);
            Scribe_Values.Look(ref percentageOfValueUsedForRansoms, "ATR_percentageOfValueUsedForRansoms", 0.25f);

            // Player Hacks
            Scribe_Values.Look(ref playerCanHack, "ATR_playerCanHack", true);
            Scribe_Values.Look(ref receiveHackingAlert, "ATR_receiveHackingAlert", true);
            Scribe_Values.Look(ref retaliationChanceOnFailure, "ATR_retaliationChanceOnFailure", 0.4f);
            Scribe_Values.Look(ref minHackSuccessChance, "ATR_minHackSuccessChance", 0.05f);
            Scribe_Values.Look(ref maxHackSuccessChance, "ATR_maxHackSuccessChance", 0.95f);

            /* === CONNECTIVITY === */

            // Skills
            Scribe_Values.Look(ref receiveSkillAlert, "ATR_receiveSkillAlert", true);
            Scribe_Values.Look(ref skillPointConversionRate, "ATR_skillPointConversionRate", 10);
            Scribe_Values.Look(ref passionSoftCap, "ATR_passionSoftCap", 8);
            Scribe_Values.Look(ref basePointsNeededForPassion, "ATR_basePointsNeededForPassion", 1000f);

            // Cloud
            Scribe_Values.Look(ref uploadingToSkyMindKills, "ATR_uploadingToSkyMindKills", true);
            Scribe_Values.Look(ref uploadingToSkyMindPermaKills, "ATR_uploadingToSkyMindPermaKills", true);
            Scribe_Values.Look(ref timeToCompleteSkyMindOperations, "ATR_timeToCompleteSkyMindOperations", 12);
        }
    }

}