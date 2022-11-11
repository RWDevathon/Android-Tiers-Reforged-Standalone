using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class ThoughtWorker_WantToSleepWithSpouseOrLover_Patch
    {
        // Mechanical and Cloud pawns don't trigger spouse/lover sleeping needs.
        [HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                Pawn otherPawn = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false).otherPawn;

                if (Utils.IsConsideredMechanical(p) || Utils.IsConsideredMechanical(otherPawn) || Utils.gameComp.GetCloudPawns().Contains(p) || Utils.gameComp.GetCloudPawns().Contains(otherPawn))
                    __result = false;
            }
        }
    }
}