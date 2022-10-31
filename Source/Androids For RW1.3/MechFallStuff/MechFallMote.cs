using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace ATReforged
{
    public static class MechFallMoteMaker
    {
        public static void MakeMechFallMote(IntVec3 cell, Map map)
        {
            Mote mote = (Mote)ThingMaker.MakeThing(RimWorld.ThingDefOf.Mote_Bombardment, null);
            mote.exactPosition = cell.ToVector3Shifted();
            mote.Scale = 5f;
            mote.rotationRate = 0f;
            GenSpawn.Spawn(mote, cell, map);
        }

    }
}
