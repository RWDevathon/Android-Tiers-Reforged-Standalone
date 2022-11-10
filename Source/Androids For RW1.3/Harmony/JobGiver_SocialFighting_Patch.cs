using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class JobGiver_SocialFighting_Patch
    {
        // Social fighting does not occur with mechanical drones. TODO: Design a way to test if anyone can reach this code if they can't do social interactions at all (IE. drones)
        [HarmonyPatch(typeof(JobGiver_SocialFighting), "TryGiveJob")]
        public class TryGiveJob_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Job __result)
            {
                Pawn otherPawn = ((MentalState_SocialFighting)pawn.MentalState).otherPawn;
                if (Utils.IsConsideredMechanicalDrone(pawn) || Utils.IsConsideredMechanicalDrone(otherPawn))
                {
                    __result = null;
                }
            }
        }
    }
}