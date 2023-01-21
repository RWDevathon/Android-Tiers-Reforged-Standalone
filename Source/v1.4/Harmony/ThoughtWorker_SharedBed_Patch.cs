using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class ThoughtWorker_SharedBed_Patch
    {
        // Mechanical drones don't care about sharing a bed. Others do mind sharing a bed with drones, but not with surrogates controlled by their lovers.
        [HarmonyPatch(typeof(ThoughtWorker_SharedBed), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if ( Utils.IsConsideredMechanicalDrone(p)
                    || Utils.IsSurrogate(p)
                    || SharingBedWithPartnerSurrogate(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }

            // Check if the shared bed is with an active surrogate of a lover. Return false if there is any non-lover surrogate.
            private static bool SharingBedWithPartnerSurrogate(Pawn p)
            {
                Building_Bed ownedBed = p.ownership.OwnedBed;
                if (ownedBed == null)
                {
                    return false;
                }
                foreach (Pawn pawn in ownedBed.OwnersForReading)
                {
                    if (!Utils.IsSurrogate(pawn) && pawn != p && !LovePartnerRelationUtility.LovePartnerRelationExists(p, pawn.GetComp<CompSkyMindLink>().GetSurrogates().FirstOrFallback()))
                    {
                        return false;
                    }
                }
                return false;
            }
        }
    }
}