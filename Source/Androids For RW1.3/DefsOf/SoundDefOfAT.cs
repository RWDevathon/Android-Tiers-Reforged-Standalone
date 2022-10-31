using System;
using RimWorld;
using Verse;

namespace ATReforged
{
    [DefOf]
    public static class SoundDefOfAT
    {
        static SoundDefOfAT()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SoundDefOfAT));
        }
        public static SoundDef Recipe_ButcherCorpseMechanoid;
        
    }
}