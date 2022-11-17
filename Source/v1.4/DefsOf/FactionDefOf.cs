using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class FactionDefOf
    {
        static FactionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
        }

        public static FactionDef AndroidUnion; // Android Outlanders

        public static FactionDef PlayerColonyAndroid; // Player

        public static FactionDef MechanicalMarauders; // Android Pirates

        // public static FactionDef AndroidPurityCouncil; // Android Supremacists
    }

}

