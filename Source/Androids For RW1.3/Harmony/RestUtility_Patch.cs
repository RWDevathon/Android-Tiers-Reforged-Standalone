using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace ATReforged
{
    internal class RestUtility_Patch

    {
        [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
        public class IsValidBedFor_Patch
        {
            // Ensure the mechanical "beds" are not occupied by non-charge-capable pawns and that mechanicals don't take non-charging beds.
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Thing bedThing, Pawn sleeper, Pawn traveler, bool checkSocialProperness, bool allowMedBedEvenIfSetToNoCare, bool ignoreOtherReservations, GuestStatus? guestStatus = null)
            {
                //Prevent extra-processing if it is already false for other reasons. 
                if (!__result)
                    return;

                if (bedThing is Building_ChargingBed)
                {
                    if (!Utils.CanUseBattery(sleeper))
                    {
                        __result = false;
                    }
                }
                else if (Utils.IsConsideredMechanical(sleeper) && bedThing.TryGetComp<CompAndroidPod>() == null)
                {
                    __result = false;
                }
            }
        }
    }
}