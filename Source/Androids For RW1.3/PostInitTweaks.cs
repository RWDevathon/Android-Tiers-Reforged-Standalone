using RimWorld;
using Verse;

namespace ATReforged
{
    // Post ThingDef creation check for Mechanical CorpseDefs.
    [StaticConstructorOnStartup]
    public static class PostInitializationTweaker
    {
        static PostInitializationTweaker()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                // Ensure only mechanical units are checked and changed.
                if (Utils.IsConsideredMechanical(thingDef))
                {
                    ThingDef corpseDef = thingDef.race?.corpseDef;
                    if (corpseDef != null)
                    {
                        // Eliminate rottable and spawnerFilth comps from mechanical corpses.
                        corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_Rottable || compProperties is CompProperties_SpawnerFilth);
                    }
                }
            }
        }
    }
}