using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using static ATReforged.Enums;

namespace ATReforged
{
    internal class FilteredGetters
    {
        public static IEnumerable<ThingDef> pregenedValidPawns;

        public static IEnumerable<ThingDef> GetValidPawns()
        {
            if (pregenedValidPawns == null)
                pregenedValidPawns = AllPawnDefs();
            return pregenedValidPawns;
        }

        // Searches through all ThingDefs to identify all Pawns, even from other mods.
        public static IEnumerable<ThingDef> AllPawnDefs()
        { 
            return DefDatabase<ThingDef>.AllDefsListForReading.Where(thingDef => thingDef.thingClass?.Name == "Pawn" && thingDef.race.intelligence != Intelligence.ToolUser);
        }

        // Return the pawn type based on the settings.
        public static IEnumerable<ThingDef> FilterByPawnKind(IEnumerable<ThingDef> options, PawnListKind pawnType)
        {
            switch (pawnType)
            {
                case PawnListKind.Android:
                    return options.Where(t => ATReforged_Settings.isConsideredMechanicalAndroid.Contains(t.defName));
                case PawnListKind.Drone:
                    return options.Where(t => ATReforged_Settings.isConsideredMechanicalDrone.Contains(t.defName));
                case PawnListKind.Animal:
                    return options.Where(t => ATReforged_Settings.isConsideredMechanicalAnimal.Contains(t.defName));
                default:
                    throw new InvalidOperationException("Attempted to filter by a nonexistant mechanical pawn type.");
            }
        }
    }
}
