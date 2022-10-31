using System;
using Verse;
using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class WorkTypeDefOf
    {
        static WorkTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WorkTypeDefOf));
        }

        public static WorkTypeDef Mechanic;
    }
}