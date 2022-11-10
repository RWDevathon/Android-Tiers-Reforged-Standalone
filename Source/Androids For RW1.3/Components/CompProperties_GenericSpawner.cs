using Verse;

namespace ATReforged
{
    public class CompProperties_GenericSpawner : CompProperties
    {
        public CompProperties_GenericSpawner()
        {
            compClass = typeof(CompSpawnerGeneric);
        }

        public PawnKindDef pawnKind;
    }
}