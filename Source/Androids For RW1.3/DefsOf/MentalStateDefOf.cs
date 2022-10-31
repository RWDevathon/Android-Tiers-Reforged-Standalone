using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace ATReforged
{
    [DefOf]
    public static class MentalStateDefOf
    {
        static MentalStateDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MentalStateDefOf));
        }
        public static MentalStateDef ATR_MentalState_Exterminator;
    }
}