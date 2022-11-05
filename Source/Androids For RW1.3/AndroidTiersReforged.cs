using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime;


using AlienRace;
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

            // Set Util Defs now that all Defs are initialized.
            Utils.SkyMindAttackWitnessDef = DefDatabase<ThoughtDef>.GetNamed("ATR_ConnectedSkyMindAttacked");
            Utils.SkyMindAttackVictimDef = DefDatabase<ThoughtDef>.GetNamed("ATR_AttackedViaSkyMind");
            Utils.SkyMindTrollVictimDef = DefDatabase<ThoughtDef>.GetNamed("ATR_TrolledViaSkyMind");

            Utils.ATR_FullChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_FullChargeFleck");
            Utils.ATR_HalfChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_HalfChargeFleck");
            Utils.ATR_EmptyChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_EmptyChargeFleck");

            // Acquire Defs for mechanical butchering so that mechanical (non-mechanoid) units are placed in the correct categories.
            RecipeDef androidDisassembly = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseMechanoid");
            RecipeDef androidSmashing = DefDatabase<RecipeDef>.GetNamed("SmashCorpseMechanoid");
            RecipeDef butcherFlesh = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh");

            // Must dynamically patch as many ThingDefs as needed that possess certain qualifications.
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                // Check race to see if the thingDef is for a Pawn.
                if (thingDef != null && thingDef.race != null)
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
                        var checker = 0;
                        foreach (var comp in thingDef.comps)
                        {
                            if (comp.compClass == typeof(CompSkyMind))
                                checker++;
                        }
                        Log.Warning("[ATR DEBUG] thingDef " + thingDef.defName + " has " + checker + " CompSkyMinds.");
                    }

                    // Mechanical pawns do not need rest or get butchered like organics do. Patch the defs at run-time because settings may change.
                    if (Utils.IsConsideredMechanical(thingDef))
                    {
                        androidDisassembly.fixedIngredientFilter.SetAllow(thingDef.race.corpseDef, true);
                        androidSmashing.fixedIngredientFilter.SetAllow(thingDef.race.corpseDef, true);
                        butcherFlesh.fixedIngredientFilter.SetAllow(thingDef.race.corpseDef, false);
                        thingDef.race.needsRest = false;
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
                            thingDef.statBases.Add(new StatModifier() { stat = RimWorld.StatDefOf.GlobalLearningFactor, value = 0 });
                        }
                    }

                    // Ensure charge-capable units have the Battery Comp.
                    if (Utils.CanUseBattery(thingDef))
                    {
                        thingDef.comps.Add(new CompProperties { compClass = typeof(CompPawnBattery) });
                    }
                    // Ensure non-mechanical units do not have the comp - they should never have one.
                    else
                    {
                        thingDef.comps.RemoveAll(compProperties => compProperties.compClass == typeof(CompPawnBattery));
                    }
                }
                // Handle SkyMind-connectable buildings.
                else if (thingDef.comps != null && !thingDef.HasComp(typeof(CompSkyMindCore)))
                {
                    bool powered = false;
                    bool flickable = false;

                    foreach (CompProperties compProp in thingDef.comps)
                    {
                        if (compProp.compClass == null)
                            continue;

                        if (compProp.compClass == typeof(CompFlickable))
                            flickable = true;
                        else if (compProp.compClass == typeof(CompPowerTrader) || (compProp.compClass == typeof(CompPowerPlant) || compProp.compClass.IsSubclassOf(typeof(CompPowerPlant))))
                        {
                            powered = true;
                        }
                    }

                    // Add CompSkyMind if it was found to be powered and flickable.
                    if (powered && flickable)
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