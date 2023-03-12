using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    public class RitualObligationTrigger_MemberDied_Patch
    {
        // Mechanical drones do not trigger death related obligations.
        [HarmonyPatch(typeof(RitualObligationTrigger_MemberDied), "Notify_MemberDied")]
        public class Notify_MemberDied_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p)
            {
                if (Utils.IsConsideredMechanicalDrone(p))
                {
                    return false;
                }
                return true;
            }
        }

    }
}