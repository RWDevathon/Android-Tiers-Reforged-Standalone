using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using static ATReforged.SettingsEnums;

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
        public static HashSet<string> blacklistedMechanicalHediffs = new HashSet<string> { };
        public static HashSet<string> blacklistedMechanicalTraits = new HashSet<string> { };
        public static bool bedRestrictionDefaultsToAll;

            // Settings for what is considered mechanical and massive
        public static bool isUsingCustomConsiderations;
        public static HashSet<string> isConsideredMechanicalAnimal;
        public static HashSet<string> isConsideredMechanicalAndroid;
        public static HashSet<string> isConsideredMechanicalDrone;
        public static HashSet<string> isConsideredMechanical;
        public static HashSet<string> hasSpecialStatus;

            // Settings for mechanical factions
        public static bool androidFactionsNeverFlee;

            // Settings for mechanical/organic rights
        public static bool factionsWillDeclareRightsWars;
        public static HashSet<string> antiMechanicalRightsFaction;
        public static HashSet<string> antiOrganicRightsFaction;
        public static bool dronesTriggerRightsWars;
        public static bool prisonersTriggerRightsWars;
        public static bool slavesTriggerRightsWars;
        public static bool surrogatesTriggerRightsWars;

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

        public static HashSet<string> canUseBattery;

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
        public static bool showMechanicalSurgerySuccessChance = false;
        public static float maxChanceMechanicOperationSuccess = 1.0f;
        public static float chanceFailedOperationMinor = 0.75f;
        public static float chancePartSavedOnFailure = 0.75f;

            // Settings for Maintenance
        public static bool maintenanceNeedExists = true;
        public static bool receiveMaintenanceFailureLetters = true;
        public static float maintenancePartFailureRateFactor = 1.0f;
        public static float maintenanceFallRateFactor = 1.0f;
        public static float maintenanceGainRateFactor = 1.0f;

        // CONNECTIVITY SETTINGS
            // Settings for Surrogates
        public static bool surrogatesAllowed = true;
        public static bool otherFactionsAllowedSurrogates = true;
        public static int minGroupSizeForSurrogates = 5;
        public static float minSurrogatePercentagePerLegalGroup = 0.2f;
        public static float maxSurrogatePercentagePerLegalGroup = 0.7f;

        public static bool displaySurrogateControlIcon = true;
        public static int safeSurrogateConnectivityCountBeforePenalty = 1;

            // Settings for Skill Points
        public static bool receiveSkillAlert = true;
        public static int skillPointInsertionRate = 100;
        public static float skillPointConversionRate = 0.5f;
        public static int passionSoftCap = 8;
        public static float basePointsNeededForPassion = 5000f;

            // Settings for Cloud
        public static bool uploadingToSkyMindKills = true;
        public static bool uploadingToSkyMindPermaKills = true;
        public static int timeToCompleteSkyMindOperations = 24;
        public static HashSet<string> factionsUsingSkyMind = new HashSet<string> { "ATR_AndroidUnion", "ATR_MechanicalMarauders" };


        // STATS SETTINGS

        // INTERNAL SETTINGS
            // Settings page
        public OptionsTab activeTab = OptionsTab.General;
        public SettingsPreset ActivePreset = SettingsPreset.None;
        public bool settingsEverOpened = false;

        public void StartupChecks()
        {
            if (isConsideredMechanicalAndroid == null)
                isConsideredMechanicalAndroid = new HashSet<string>();
            if (isConsideredMechanicalDrone == null)
                isConsideredMechanicalDrone = new HashSet<string>();
            if (isConsideredMechanicalAnimal == null)
                isConsideredMechanicalAnimal = new HashSet<string>();
            if (isConsideredMechanical == null)
                isConsideredMechanical = new HashSet<string>();
            if (hasSpecialStatus == null)
                hasSpecialStatus = new HashSet<string>();
            if (antiMechanicalRightsFaction == null)
                antiMechanicalRightsFaction = new HashSet<string>();
            if (antiOrganicRightsFaction == null)
                antiOrganicRightsFaction = new HashSet<string>();
            if (canUseBattery == null)
                canUseBattery = new HashSet<string>();
            if (ActivePreset == SettingsPreset.None)
            {
                settingsEverOpened = false;
                ApplyPreset(SettingsPreset.Default);
            }
            if (!isUsingCustomConsiderations)
            {
                RebuildCaches();
            }
        }

        Vector2 scrollPosition = Vector2.zero;
        float cachedScrollHeight = 0;

        bool cachedExpandFirst = true;
        bool cachedExpandSecond = true;
        bool cachedExpandThird = true;
        bool cachedExpandFourth = false;

        void ResetCachedExpand() 
        { 
            cachedExpandFirst = true; 
            cachedExpandSecond = true; 
            cachedExpandThird = true;
            cachedExpandFourth = false;
        }

        internal void DoSettingsWindowContents(Rect inRect)
        {
            settingsEverOpened = true;
            bool hasChanged = false;
            void onChange() { hasChanged = true; }
            void onConsiderationChange()
            {
                onChange();
                isUsingCustomConsiderations = true;
            }

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
                    { 
                        bool fixedGender = androidsFixedGender == Gender.Female;
                        listingStandard.CheckboxLabeled("ATR_AndroidsFixedGenderSelector".Translate(), ref fixedGender, tooltip: "ATR_AndroidGenderNotice".Translate(), onChange: onChange);
                        androidsFixedGender = fixedGender ? Gender.Female: Gender.Male;
                    }

                    if (androidsHaveGenders && androidsPickGenders)
                    { 
                        listingStandard.SliderLabeled("ATR_AndroidsGenderRatio".Translate(), ref androidsGenderRatio, 0.0f, 1.0f, displayMult: 100, onChange: onChange);
                    }
                    listingStandard.GapLine();

                    // PERMISSION SETTINGS
                    listingStandard.CheckboxLabeled("ATR_bedRestrictionDefaultsToAll".Translate(), ref bedRestrictionDefaultsToAll, tooltip: "ATR_bedRestrictionDefaultsToAllDesc".Translate(), onChange: onChange);
                    listingStandard.GapLine();

                    // CONSIDERATION SETTINGS
                    listingStandard.Label("ATR_RestartRequiredSectionDesc".Translate());
                    if (listingStandard.ButtonTextLabeled("ATR_isUsingCustomConsiderations".Translate(isUsingCustomConsiderations.ToString()), "ATR_resetCustomConsiderations".Translate(), tooltip: "ATR_isUsingCustomConsiderationsDesc".Translate()))
                    {
                            RebuildCaches();
                            isUsingCustomConsiderations = false;
                    }

                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                            cachedExpandFirst = !cachedExpandFirst;
                    }
                    if (cachedExpandFirst)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Humanlike), isConsideredMechanicalAndroid, "ATR_SettingsConsideredAndroid".Translate(), "ATR_SettingsNotConsideredAndroid".Translate(), onConsiderationChange);
                    
                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                        cachedExpandSecond = !cachedExpandSecond;
                    }
                    if (cachedExpandSecond)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Humanlike), isConsideredMechanicalDrone, "ATR_SettingsConsideredDrone".Translate(), "ATR_SettingsNotConsideredDrone".Translate(), onConsiderationChange);
                    
                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                        cachedExpandThird = !cachedExpandThird;
                    }
                    if (cachedExpandThird)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Animal), isConsideredMechanicalAnimal, "ATR_SettingsConsideredAnimal".Translate(), "ATR_SettingsNotConsideredAnimals".Translate(), onConsiderationChange);
                    
                    listingStandard.GapLine();

                    // ANDROID FACTION SETTINGS
                    listingStandard.CheckboxLabeled("ATR_AndroidFactionsNeverFlee".Translate(), ref androidFactionsNeverFlee, onChange: onChange);
                    listingStandard.GapLine();

                    // RIGHTS SETTINGS

                    listingStandard.CheckboxLabeled("ATR_factionsWillDeclareRightsWars".Translate(), ref factionsWillDeclareRightsWars, tooltip: "ATR_factionsWillDeclareRightsWarsDesc".Translate(), onChange: onChange);
                    if (factionsWillDeclareRightsWars && listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                        cachedExpandFourth = !cachedExpandFourth;
                    }
                    if (factionsWillDeclareRightsWars && cachedExpandFourth)
                    {
                        listingStandard.DefSelector(DefDatabase<FactionDef>.AllDefsListForReading, ref antiMechanicalRightsFaction, "ATR_SettingsAntiMechanicalFaction".Translate(), "ATR_SettingsTolerateMechanicalFaction".Translate(), onChange);
                        listingStandard.DefSelector(DefDatabase<FactionDef>.AllDefsListForReading, ref antiOrganicRightsFaction, "ATR_SettingsAntiOrganicFaction".Translate(), "ATR_SettingsTolerateOrganicFaction".Translate(), onChange);
                    }
                    if (factionsWillDeclareRightsWars)
                    {
                        listingStandard.CheckboxLabeled("ATR_dronesTriggerRightsWars".Translate(), ref dronesTriggerRightsWars, onChange: onChange);
                        listingStandard.CheckboxLabeled("ATR_prisonersTriggerRightsWars".Translate(), ref prisonersTriggerRightsWars, onChange: onChange);
                        listingStandard.CheckboxLabeled("ATR_slavesTriggerRightsWars".Translate(), ref slavesTriggerRightsWars, onChange: onChange);
                        listingStandard.CheckboxLabeled("ATR_surrogatesTriggerRightsWars".Translate(), ref surrogatesTriggerRightsWars, onChange: onChange);
                    }
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
                case OptionsTab.Health:
                {
                    // MEDICAL
                    listingStandard.CheckboxLabeled("ATR_medicinesAreInterchangeable".Translate(), ref medicinesAreInterchangeable, onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_showMechanicalSurgerySuccessChance".Translate(), ref showMechanicalSurgerySuccessChance, onChange: onChange);
                    listingStandard.SliderLabeled("ATR_maxChanceMechanicOperationSuccess".Translate(), ref maxChanceMechanicOperationSuccess, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    listingStandard.SliderLabeled("ATR_chanceFailedOperationMinor".Translate(), ref chanceFailedOperationMinor, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    listingStandard.SliderLabeled("ATR_chancePartSavedOnFailure".Translate(), ref chancePartSavedOnFailure, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    listingStandard.GapLine();

                    // MAINTENANCE
                    listingStandard.CheckboxLabeled("ATR_maintenanceNeedExists".Translate(), ref maintenanceNeedExists, onChange: onChange);
                    if (maintenanceNeedExists)
                    {
                        listingStandard.CheckboxLabeled("ATR_receiveMaintenanceFailureLetters".Translate(), ref receiveMaintenanceFailureLetters, onChange: onChange);
                        listingStandard.SliderLabeled("ATR_maintenancePartFailureRateFactor".Translate(), ref maintenancePartFailureRateFactor, 0.5f, 5f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        listingStandard.SliderLabeled("ATR_maintenanceFallRateFactor".Translate(), ref maintenanceFallRateFactor, 0.5f, 5f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        listingStandard.SliderLabeled("ATR_maintenanceGainRateFactor".Translate(), ref maintenanceGainRateFactor, 0.5f, 5f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    }
                    listingStandard.GapLine();

                    // HEDIFFS
                    listingStandard.Label("ATR_hediffBlacklistWarning".Translate());
                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                        cachedExpandFirst = !cachedExpandFirst;
                    }
                    if (!cachedExpandFirst)
                    {
                        listingStandard.DefSelector(DefDatabase<HediffDef>.AllDefsListForReading, ref blacklistedMechanicalHediffs, "ATR_settingsBlacklistedMechanicalHediffs".Translate(), "ATR_settingsAllowedMechanicalHediffs".Translate(), onChange);
                    }
                    listingStandard.GapLine();

                    break;
                }
                case OptionsTab.Connectivity:
                {
                    // SURROGATES
                    listingStandard.CheckboxLabeled("ATR_surrogatesAllowed".Translate(), ref surrogatesAllowed, onChange: onChange);
                    if (surrogatesAllowed)
                    {
                        listingStandard.CheckboxLabeled("ATR_otherFactionsAllowedSurrogates".Translate(), ref otherFactionsAllowedSurrogates, onChange: onChange);
                        if (otherFactionsAllowedSurrogates)
                        {
                            string minGroupSizeForSurrogatesBuffer = minGroupSizeForSurrogates.ToString();
                            listingStandard.TextFieldNumericLabeled("ATR_minGroupSizeForSurrogates".Translate(), ref minGroupSizeForSurrogates, ref minGroupSizeForSurrogatesBuffer, 1, 50);
                            listingStandard.SliderLabeled("ATR_minSurrogatePercentagePerLegalGroup".Translate(), ref minSurrogatePercentagePerLegalGroup, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                            listingStandard.SliderLabeled("ATR_maxSurrogatePercentagePerLegalGroup".Translate(), ref maxSurrogatePercentagePerLegalGroup, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        }
                        listingStandard.CheckboxLabeled("ATR_displaySurrogateControlIcon".Translate(), ref displaySurrogateControlIcon, onChange: onChange);
                        string safeSurrogateConnectivityCountBeforePenaltyBuffer = safeSurrogateConnectivityCountBeforePenalty.ToString();
                        listingStandard.TextFieldNumericLabeled("ATR_safeSurrogateConnectivityCountBeforePenalty".Translate(), ref safeSurrogateConnectivityCountBeforePenalty, ref safeSurrogateConnectivityCountBeforePenaltyBuffer, 1, 40);
                    }
                    listingStandard.GapLine();

                    // SKILL POINTS
                    string skillPointInsertionRateBuffer = skillPointInsertionRate.ToString();
                    string skillPointConversionRateBuffer = skillPointConversionRate.ToString();
                    string passionSoftCapBuffer = passionSoftCap.ToString();
                    string basePointsNeededForPassionBuffer = basePointsNeededForPassion.ToString();
                    listingStandard.CheckboxLabeled("ATR_receiveFullSkillAlert".Translate(), ref receiveSkillAlert, onChange: onChange);
                    listingStandard.TextFieldNumericLabeled("ATR_skillPointInsertionRate".Translate(), ref skillPointInsertionRate, ref skillPointInsertionRateBuffer, 1f);
                    listingStandard.TextFieldNumericLabeled("ATR_skillPointConversionRate".Translate(), ref skillPointConversionRate, ref skillPointConversionRateBuffer, 0.01f, 10);
                    listingStandard.TextFieldNumericLabeled("ATR_passionSoftCap".Translate(), ref passionSoftCap, ref passionSoftCapBuffer, 0, 50);
                    listingStandard.TextFieldNumericLabeled("ATR_basePointsNeededForPassion".Translate(), ref basePointsNeededForPassion, ref basePointsNeededForPassionBuffer, 10, 10000);
                    listingStandard.GapLine();

                    // CLOUD
                    listingStandard.CheckboxLabeled("ATR_UploadingKills".Translate(), ref uploadingToSkyMindKills, onChange: onChange);
                    listingStandard.CheckboxLabeled("ATR_UploadingPermakills".Translate(), ref uploadingToSkyMindPermaKills, onChange: onChange);
                    string SkyMindOperationTimeBuffer = timeToCompleteSkyMindOperations.ToString();
                    listingStandard.TextFieldNumericLabeled("ATR_SkyMindOperationTimeRequired".Translate(), ref timeToCompleteSkyMindOperations, ref SkyMindOperationTimeBuffer, 1, 256);
                    listingStandard.GapLine();

                    break;
                }
                case OptionsTab.Stats:
                {
                    // Traits
                    listingStandard.Label("ATR_traitBlacklistWarning".Translate());
                    if (listingStandard.ButtonText("ATR_ExpandMenu".Translate()))
                    {
                        cachedExpandFirst = !cachedExpandFirst;
                    }
                    if (!cachedExpandFirst)
                    {
                        listingStandard.DefSelector(DefDatabase<TraitDef>.AllDefsListForReading, ref blacklistedMechanicalTraits, "ATR_settingsBlacklistedMechanicalTraits".Translate(), "ATR_settingsAllowedMechanicalTraits".Translate(), onChange);
                    }
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
            blacklistedMechanicalHediffs = new HashSet<string> { };
            blacklistedMechanicalTraits = new HashSet<string> { };
            bedRestrictionDefaultsToAll = false;

            // Considerations
            isUsingCustomConsiderations = false;

            // Android Factions
            androidFactionsNeverFlee = false;

            // Rights
            factionsWillDeclareRightsWars = true;
            antiMechanicalRightsFaction = new HashSet<string> { "Empire" };
            antiOrganicRightsFaction = new HashSet<string> { "ATR_MechanicalMarauders" };
            dronesTriggerRightsWars = true;
            prisonersTriggerRightsWars = false;
            slavesTriggerRightsWars = true;
            surrogatesTriggerRightsWars = true;

            // Needs Settings
            androidsHaveJoyNeed = true;
            androidsHaveBeautyNeed = true;
            androidsHaveComfortNeed = true;
            androidsHaveOutdoorsNeed = true;

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

            // HEALTH SETTINGS
                // Medical
            medicinesAreInterchangeable = false;
            showMechanicalSurgerySuccessChance = false;
            maxChanceMechanicOperationSuccess = 1f;
            chanceFailedOperationMinor = 0.75f;
            chancePartSavedOnFailure = 0.75f;

            // Maintenance
            maintenanceNeedExists = true;
            receiveMaintenanceFailureLetters = true;
            maintenancePartFailureRateFactor = 1.0f;
            maintenanceFallRateFactor = 1.0f;
            maintenanceGainRateFactor = 1.0f;

            // CONNECTIVITY SETTINGS
                // Surrogates
            surrogatesAllowed = true;
            otherFactionsAllowedSurrogates = true;
            minGroupSizeForSurrogates = 5;
            minSurrogatePercentagePerLegalGroup = 0.2f;
            maxSurrogatePercentagePerLegalGroup = 0.7f;
            displaySurrogateControlIcon = true;
            safeSurrogateConnectivityCountBeforePenalty = 1;

            // Skills
            skillPointInsertionRate = 100;
            skillPointConversionRate = 0.5f;
            passionSoftCap = 8;
            basePointsNeededForPassion = 5000f;

            // Cloud
            receiveSkillAlert = true;
            uploadingToSkyMindKills = true;
            uploadingToSkyMindPermaKills = true;
            timeToCompleteSkyMindOperations = 24;

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

            HashSet<string> matchingAndroids = new HashSet<string>();
            HashSet<string> matchingDrones = new HashSet<string>();
            HashSet<string> matchingMechanicals = new HashSet<string>();
            HashSet<string> matchingSpecials = new HashSet<string>();
            HashSet<string> matchingChargers = new HashSet<string>();
            foreach (ThingDef validHumanlike in FilteredGetters.FilterByIntelligence(validPawns, Intelligence.Humanlike).Where(thingDef => thingDef.HasModExtension<ATR_MechTweaker>()))
            {
                ATR_MechTweaker modExt = validHumanlike.GetModExtension<ATR_MechTweaker>();
                // Mechanical Androids are humanlikes with global learning factor >= 0.5 that have the ModExtension. Or are simply marked as canBeAndroid and not canBeDrone.
                if (modExt.canBeAndroid && (validHumanlike.statBases?.GetStatValueFromList(StatDefOf.GlobalLearningFactor, 0.5f) >= 0.5f || !modExt.canBeDrone))
                {
                    matchingAndroids.Add(validHumanlike.defName);
                    // A special bool in the mod extension marks this as a special android.
                    if (modExt.isSpecialMechanical)
                        matchingSpecials.Add(validHumanlike.defName);

                    // All mechanical humanlikes may charge inherently.
                    matchingChargers.Add(validHumanlike.defName);
                    matchingMechanicals.Add(validHumanlike.defName);
                }
                // Mechanical Drones are humanlikes with global learning factor < 0.5 that have the ModExtension. Or are simply marked as canBeDrone and not canBeAndroid.
                else if (modExt.canBeDrone && (validHumanlike.statBases?.GetStatValueFromList(StatDefOf.GlobalLearningFactor, 0.5f) < 0.5f || !modExt.canBeAndroid))
                {
                    matchingDrones.Add(validHumanlike.defName);
                    // All mechanical humanlikes may charge inherently.
                    matchingChargers.Add(validHumanlike.defName);
                    matchingMechanicals.Add(validHumanlike.defName);
                }
                else
                {
                    Log.Warning("[ATR] A humanlike race " + validHumanlike + " with the ATR_MechTweaker mod extension was unable to automatically select its categorization! This will leave it as being considered organic.");
                }
            }
            // Mechanical animals are animals that have the ModExtension
            HashSet<ThingDef> validAnimals = FilteredGetters.FilterByIntelligence(validPawns, Intelligence.Animal).Where(thingDef => thingDef.HasModExtension<ATR_MechTweaker>()).ToHashSet();
            HashSet<string> matchingAnimals = new HashSet<string>();

            // Mechanical animals of advanced intelligence may charge.
            foreach (ThingDef validAnimal in validAnimals)
            {
                matchingAnimals.Add(validAnimal.defName);
                matchingMechanicals.Add(validAnimal.defName);
                // Advanced mechanical animals may charge.
                if (validAnimal.race.trainability == TrainabilityDefOf.Advanced)
                    matchingChargers.Add(validAnimal.defName);
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
            Scribe_Values.Look(ref bedRestrictionDefaultsToAll, "ATR_bedRestrictionDefaultsToAll", false);

            // Considerations
            Scribe_Values.Look(ref isUsingCustomConsiderations, "ATR_isUsingCustomConsiderations", true); // TODO: after a critical save-break update, set this to false for the future.
            try
            {
                Scribe_Collections.Look(ref isConsideredMechanicalAnimal, "ATR_isConsideredMechanicalAnimal", LookMode.Value);
                Scribe_Collections.Look(ref isConsideredMechanicalAndroid, "ATR_isConsideredMechanicalAndroid", LookMode.Value);
                Scribe_Collections.Look(ref isConsideredMechanicalDrone, "ATR_isConsideredMechanicalDrone", LookMode.Value);
                Scribe_Collections.Look(ref isConsideredMechanical, "ATR_isConsideredMechanical", LookMode.Value);
                Scribe_Collections.Look(ref hasSpecialStatus, "ATR_hasSpecialStatus", LookMode.Value);
            }
            catch (Exception ex)
            {
                Log.Warning("[ATR] Mod settings failed to load appropriately! Resetting to default to avoid further issues. " + ex.Message + " " + ex.StackTrace);
                RebuildCaches();
            }

            // Android Factions
            Scribe_Values.Look(ref androidFactionsNeverFlee, "ATR_androidFactionsNeverFlee", false);

            // Rights
            Scribe_Values.Look(ref factionsWillDeclareRightsWars, "ATR_factionsWillDeclareRightsWars", true);
            Scribe_Collections.Look(ref antiMechanicalRightsFaction, "ATR_antiMechanicalRightsFaction", LookMode.Value);
            Scribe_Collections.Look(ref antiOrganicRightsFaction, "ATR_antiOrganicRightsFaction", LookMode.Value);
            Scribe_Values.Look(ref dronesTriggerRightsWars, "ATR_dronesTriggerRightsWars", true);
            Scribe_Values.Look(ref prisonersTriggerRightsWars, "ATR_prisonersTriggerRightsWars", false);
            Scribe_Values.Look(ref slavesTriggerRightsWars, "ATR_slavesTriggerRightsWars", true);
            Scribe_Values.Look(ref surrogatesTriggerRightsWars, "ATR_surrogatesTriggerRightsWars", true);

            // Needs
            Scribe_Values.Look(ref androidsHaveJoyNeed, "ATR_androidsHaveJoyNeed", true);
            Scribe_Values.Look(ref androidsHaveBeautyNeed, "ATR_androidsHaveBeautyNeed", true);
            Scribe_Values.Look(ref androidsHaveComfortNeed, "ATR_androidsHaveComfortNeed", true);
            Scribe_Values.Look(ref androidsHaveOutdoorsNeed, "ATR_androidsHaveOutdoorsNeed", true);

            /* === POWER === */

            Scribe_Values.Look(ref wattsConsumedPerBodySize, "ATR_wattsConsumedPerBodySize", 500);
            Scribe_Values.Look(ref chargeCapableMeansDifferentBioEfficiency, "ATR_chargeCapableMeansDifferentBioEfficiency", true);
            Scribe_Values.Look(ref chargeCapableBioEfficiency, "ATR_chargeCapableBioEfficiency", 0.5f);
            Scribe_Values.Look(ref batteryChargeRate, "ATR_batteryChargeRate", 1f);

            try
            {
                Scribe_Collections.Look(ref canUseBattery, "ATR_canUseBattery", LookMode.Value);
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

            /* === HEALTH === */
            // Medical
            Scribe_Values.Look(ref medicinesAreInterchangeable, "ATR_medicinesAreInterchangeable", false);
            Scribe_Values.Look(ref showMechanicalSurgerySuccessChance, "ATR_showMechanicalSurgerySuccessChance", false);
            Scribe_Values.Look(ref maxChanceMechanicOperationSuccess, "ATR_maxChanceMechanicOperationSuccess", 1f);
            Scribe_Values.Look(ref chanceFailedOperationMinor, "ATR_chanceFailedOperationMinor", 0.75f);
            Scribe_Values.Look(ref chancePartSavedOnFailure, "ATR_chancePartSavedOnFailure", 0.75f);

            // Maintenance
            Scribe_Values.Look(ref maintenanceNeedExists, "ATR_maintenanceNeedExists", true);
            Scribe_Values.Look(ref receiveMaintenanceFailureLetters, "ATR_receiveMaintenanceFailureLetters", true);
            Scribe_Values.Look(ref maintenancePartFailureRateFactor, "ATR_maintenancePartFailureRateFactor", 1.0f);
            Scribe_Values.Look(ref maintenanceFallRateFactor, "ATR_maintenanceFallRateFactor", 1.0f);
            Scribe_Values.Look(ref maintenanceGainRateFactor, "ATR_maintenanceGainRateFactor", 1.0f);

            /* === CONNECTIVITY === */
            // Surrogates
            Scribe_Values.Look(ref surrogatesAllowed, "ATR_surrogatesAllowed", true);
            Scribe_Values.Look(ref otherFactionsAllowedSurrogates, "ATR_otherFactionsAllowedSurrogates", true);
            Scribe_Values.Look(ref minGroupSizeForSurrogates, "ATR_minGroupSizeForSurrogates", 5);
            Scribe_Values.Look(ref minSurrogatePercentagePerLegalGroup, "ATR_minSurrogatePercentagePerLegalGroup", 0.2f);
            Scribe_Values.Look(ref maxSurrogatePercentagePerLegalGroup, "ATR_maxSurrogatePercentagePerLegalGroup", 0.7f);
            Scribe_Values.Look(ref displaySurrogateControlIcon, "ATR_displaySurrogateControlIcon", true);
            Scribe_Values.Look(ref safeSurrogateConnectivityCountBeforePenalty, "ATR_safeSurrogateConnectivityCountBeforePenalty", 1);

            // Skills
            Scribe_Values.Look(ref receiveSkillAlert, "ATR_receiveSkillAlert", true);
            Scribe_Values.Look(ref skillPointInsertionRate, "ATR_skillPointInsertionRate", 100);
            Scribe_Values.Look(ref skillPointConversionRate, "ATR_skillPointConversionRate", 0.5f);
            Scribe_Values.Look(ref passionSoftCap, "ATR_passionSoftCap", 8);
            Scribe_Values.Look(ref basePointsNeededForPassion, "ATR_basePointsNeededForPassion", 5000f);

            // Cloud
            Scribe_Values.Look(ref uploadingToSkyMindKills, "ATR_uploadingToSkyMindKills", true);
            Scribe_Values.Look(ref uploadingToSkyMindPermaKills, "ATR_uploadingToSkyMindPermaKills", true);
            Scribe_Values.Look(ref timeToCompleteSkyMindOperations, "ATR_timeToCompleteSkyMindOperations", 24);
        }
    }

}