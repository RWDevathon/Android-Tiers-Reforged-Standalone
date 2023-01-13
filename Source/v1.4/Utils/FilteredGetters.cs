using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ATReforged
{
    internal class FilteredGetters
    {
        public static IEnumerable<ThingDef> pregenedValidPawns;

        public static IEnumerable<ThingDef> GetValidPawns()
        {
            if (pregenedValidPawns == null)
                pregenedValidPawns = AllValidPawnDefs();
            return pregenedValidPawns;
        }

        // Returns an IEnumerable<ThingDef> containing all ThingDefs for the provided IEnumerable<string> defNames
        public static IEnumerable<ThingDef> GetThingDefsFromDefNames(HashSet<string> defNames)
        {
            return DefDatabase<ThingDef>.AllDefsListForReading.Where(thingDef => defNames.Contains(thingDef.defName));
        }

        // Searches through all ThingDefs to identify all Pawns valid for this mod's needs, even from other mods.
        public static IEnumerable<ThingDef> AllValidPawnDefs()
        { 
            return DefDatabase<ThingDef>.AllDefsListForReading.Where(thingDef => thingDef.thingClass?.Name == "Pawn" && thingDef.race.intelligence != Intelligence.ToolUser && !thingDef.race.IsMechanoid);
        }

        // Return an enumerable of pawns based on the given intelligence.
        public static IEnumerable<ThingDef> FilterByIntelligence(IEnumerable<ThingDef> options, Intelligence intelligence)
        {
            return options.Where(thingDef => thingDef.race.intelligence == intelligence);
        }
    }
}
