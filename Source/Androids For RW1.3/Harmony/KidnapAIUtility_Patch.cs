using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    internal class KidnapAIUtility_Patch

    {
        // There's no point in kidnapping surrogates. If one is selected, choose something else instead.
        [HarmonyPatch(typeof(KidnapAIUtility), "TryFindGoodKidnapVictim")]
        public class TryFindGoodKidnapVictim_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Pawn kidnapper, float maxDist, ref Pawn victim, List<Thing> disallowed = null)
            {
                if(__result && Utils.IsSurrogate(victim))
                {
                    bool validator(Thing t)
                    {
                        Pawn pawn = t as Pawn;
                        return pawn.RaceProps.Humanlike && pawn.Downed && pawn.Faction == Faction.OfPlayer && !Utils.IsSurrogate(pawn) && pawn.Faction.HostileTo(kidnapper.Faction) && kidnapper.CanReserve(pawn, 1, -1, null, false) && (disallowed == null || !disallowed.Contains(pawn));
                    }
                    victim = (Pawn)GenClosest.ClosestThingReachable(kidnapper.Position, kidnapper.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some, false), maxDist, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                    __result = victim != null;
                }
            }
        }
    }
}