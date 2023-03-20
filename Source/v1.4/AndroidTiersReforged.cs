using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld;

namespace ATReforged
{
    public class ATReforged : Mod
    {
        public static ATReforged_Settings settings;
        public static ATReforged ModSingleton { get; private set; }

        public ATReforged(ModContentPack content) : base(content)
        {
            ModSingleton = this;
            new Harmony("ATReforged").PatchAll(Assembly.GetExecutingAssembly());
        }
        
        // Handles the localization for the mod's name in the list of mods in the mod settings page.
        public override string SettingsCategory()
        {
            return "ATR_ModTitle".Translate();
        }

        // Handles actually displaying this mod's settings.
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }

    [StaticConstructorOnStartup]
    public static class ATReforged_PostInit
    {
        static ATReforged_PostInit()
        {
            ATReforged.settings = ATReforged.ModSingleton.GetSettings<ATReforged_Settings>();
            ATReforged.settings.StartupChecks();

            // Patch android factions based on the appropriate settings.
            DefDatabase<FactionDef>.GetNamedSilentFail("ATR_AndroidUnion").autoFlee = ATReforged_Settings.androidFactionsNeverFlee;
            DefDatabase<FactionDef>.GetNamedSilentFail("ATR_MechanicalMarauders").autoFlee = ATReforged_Settings.androidFactionsNeverFlee;

            // Acquire Defs for mechanical butchering so that mechanical (non-mechanoid) units are placed in the correct categories.
            RecipeDef androidDisassembly = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseMechanoid");
            RecipeDef androidSmashing = DefDatabase<RecipeDef>.GetNamed("SmashCorpseMechanoid");
            RecipeDef butcherFlesh = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh");

            CompProperties_Facility bedsideChargerLinkables = ATR_ThingDefOf.ATR_BedsideChargerFacility.GetCompProperties<CompProperties_Facility>();

            // Some patches can't be run with the other harmony patches as Defs aren't loaded yet. So we patch them here.
            if (HealthCardUtility_Patch.DrawOverviewTab_Patch.Prepare())
            {
                new Harmony("ATReforged").CreateClassProcessor(typeof(HealthCardUtility_Patch.DrawOverviewTab_Patch)).Patch();
            }

            // Must dynamically modify some ThingDefs based on certain qualifications.
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                // Check race to see if the thingDef is for a Pawn.
                if (thingDef.race != null)
                {
                    // Humanlikes get specific comps for SkyMind related things.
                    if (thingDef.race.intelligence == Intelligence.Humanlike)
                    {
                        CompProperties cp;
                        cp = new CompProperties
                        {
                            compClass = typeof(CompSkyMind)
                        };
                        thingDef.comps.Add(cp);

                        cp = new CompProperties
                        {
                            compClass = typeof(CompSkyMindLink)
                        };
                        thingDef.comps.Add(cp);
                    }

                    // Mechanical pawns do not need rest or get butchered like organics do. Mechanical pawns get the maintenance need unless they have a mod extension that prevents it.
                    if (Utils.IsConsideredMechanical(thingDef))
                    {
                        ThingDef corpseDef = thingDef.race?.corpseDef;
                        if (corpseDef != null)
                        {
                            // Eliminate rottable and spawnerFilth comps from mechanical corpses.
                            corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_Rottable || compProperties is CompProperties_SpawnerFilth);

                            // Put android disassembly in the machining table and crafting spot (smashing) and remove from the butcher table.
                            androidDisassembly.fixedIngredientFilter.SetAllow(corpseDef, true);
                            androidSmashing.fixedIngredientFilter.SetAllow(corpseDef, true);
                            butcherFlesh.fixedIngredientFilter.SetAllow(corpseDef, false);

                            // Make android corpses not edible.
                            IngestibleProperties ingestibleProps = corpseDef.ingestible;
                            if (ingestibleProps != null)
                            {
                                ingestibleProps.preferability = FoodPreferability.Undefined;
                            }
                        }

                        if (thingDef.GetModExtension<ATR_MechTweaker>()?.needsMaintenance == true && ATReforged_Settings.maintenanceNeedExists)
                        {
                            CompProperties cp = new CompProperties
                            {
                                compClass = typeof(CompMaintenanceNeed)
                            };
                            thingDef.comps.Add(cp);
                        }
                    }

                    // Drones do not have learning factors.
                    if (Utils.IsConsideredMechanicalDrone(thingDef))
                    {
                        StatModifier learningModifier = thingDef.statBases.Find(modifier => modifier.stat.defName == "GlobalLearningFactor");
                        if (learningModifier != null)
                        {
                            learningModifier.value = 0;
                        }
                        else
                        {
                            thingDef.statBases.Add(new StatModifier() { stat = StatDefOf.GlobalLearningFactor, value = 0 });
                        }
                    }
                }
                // Handle SkyMind-connectable buildings.
                else if (typeof(Building).IsAssignableFrom(thingDef.thingClass) && thingDef.comps != null)
                {
                    foreach (CompProperties compProp in thingDef.comps)
                    {
                        // Add CompSkyMind if it can be powered.
                        if (compProp.compClass?.IsAssignableFrom(typeof(CompPowerTrader)) == true)
                        {
                            CompProperties cp = new CompProperties
                            {
                                compClass = typeof(CompSkyMind)
                            };
                            thingDef.comps.Add(cp);

                            // Autodoors get a special comp to allow them to be opened/closed remotely.
                            if (thingDef.IsDoor)
                            {
                                cp = new CompProperties
                                {
                                    compClass = typeof(CompAutoDoor)
                                };
                                thingDef.comps.Add(cp);
                            }

                            // Research benches get a special comp to control what server type it can be used to generate points for.
                            if (typeof(Building_ResearchBench).IsAssignableFrom(thingDef.thingClass))
                            {
                                cp = new CompProperties
                                {
                                    compClass = typeof(CompInsightBench)
                                };
                                thingDef.comps.Add(cp);
                            }
                            break;
                        }
                    }
                    // Explosive traps may be connected to the SkyMind network for remote triggering
                    if (typeof(Building_TrapExplosive).IsAssignableFrom(thingDef.thingClass))
                    {
                        CompProperties cp;

                        // If it didn't get a CompSkyMind from the previous step because it has no CompPowerTrader, add one now.
                        if (!thingDef.HasComp(typeof(CompPowerTrader)))
                        {
                            cp = new CompProperties
                            {
                                compClass = typeof(CompSkyMind)
                            };
                            thingDef.comps.Add(cp);
                        }

                        cp = new CompProperties
                        {
                            compClass = typeof(CompRemotelyTriggered)
                        };
                        thingDef.comps.Add(cp);
                    }
                }
                // All beds should have the Restrictable comp to restrict what pawn type may use it.
                if (thingDef.IsBed)
                {
                    CompProperties cp = new CompProperties
                    {
                        compClass = typeof(CompPawnTypeRestrictable)
                    };
                    thingDef.comps.Add(cp);

                    // Non-charging beds also should have the bedside charger as a linkable building.
                    CompProperties_AffectedByFacilities linkable = thingDef.GetCompProperties<CompProperties_AffectedByFacilities>();
                    if (linkable != null && !typeof(Building_ChargingBed).IsAssignableFrom(thingDef.thingClass))
                    {
                        linkable.linkableFacilities.Add(ATR_ThingDefOf.ATR_BedsideChargerFacility);
                        bedsideChargerLinkables.linkableBuildings.Add(thingDef);
                    }
                }
            }

            // Utils needs a list of viable PawnKindDefs for surrogates. Seek all BackstoryFilterOverrides that use "SurrogateSoldier" and use them.
            List<PawnKindDef> validSurrogates = new List<PawnKindDef>();
            foreach (PawnKindDef entry in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (entry.backstoryFiltersOverride != null)
                {
                    foreach (BackstoryCategoryFilter backstoryFilter in entry.backstoryFiltersOverride)
                    {
                        if (backstoryFilter.categories != null && backstoryFilter.categories.Contains("SurrogateSoldier"))
                        {
                            validSurrogates.Add(entry);
                        }
                    }
                }
            }
            Utils.ValidSurrogatePawnKindDefs = validSurrogates;
        }
    }
}