using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime;


using AlienRace;

namespace ATReforged
{
    public class ATReforged : Mod
    {
        public ATReforged_Settings settings;

        public ATReforged(ModContentPack content) : base(content)
        {
            settings = GetSettings<ATReforged_Settings>();
            settings.StartupChecks();

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
}