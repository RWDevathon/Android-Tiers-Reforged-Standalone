using System;
using RimWorld.Planet;
using RimWorld;

namespace ATReforged
{
    public class WorldObjectCompProperties_DownedT5Android : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_DownedT5Android()
        { // TODO: Fix whatever is going on here with world comp properties for the T5 android.
            this.compClass = typeof(DownedT5AndroidComp);
            this.compClass = typeof(TimedForcedExit);
        }
    }
}