using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class Precept_Ritual_Patch
    {
        // Mechanical drones do not have ideological obligations.
        [HarmonyPatch(typeof(Precept_Ritual), "AddObligation")]
        public class AddObligation_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(RitualObligation obligation)
            {
                if (obligation.targetA.Thing == null
                    || (obligation.targetA.Thing is Pawn pawn && Utils.IsConsideredMechanicalDrone(pawn))
                    || (obligation.targetA.Thing is Corpse corpse && Utils.IsConsideredMechanicalDrone(corpse.InnerPawn)))
                {
                    return false;
                }
                return true;
            }
        }

    }
}