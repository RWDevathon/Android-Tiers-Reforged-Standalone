using Verse;
using HarmonyLib;
using RimWorld;
using System;
using RimWorld.Planet;

namespace ATReforged
{
    internal class Pawn_HealthTracker_Patch
    {
        // Ensure the hediff to be added is not forbidden on the given pawn (for mechanicals) before doing standard AddHediff checks - it would be wasted/junk calculations.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff")]
        [HarmonyPatch(new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
        public class AddHediff_Patch
        { 
            [HarmonyPrefix]
            public static bool Listener(ref Pawn ___pawn, ref Hediff hediff, BodyPartRecord part)
            {
                // If this is a mechanical pawn and this particular hediff is forbidden for mechanicals to have, then abort trying to add it.
                if (Utils.IsConsideredMechanical(___pawn) && ATReforged_Settings.blacklistedMechanicalHediffs.Contains(hediff.def.defName))
                {
                    return false;
                }

                return true;
            }
        }

        // Upon a pawn being downed, some pawns should die automatically, and enemy surrogates brains explode.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
        public static class DiesUponDowned_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn_HealthTracker __instance, DamageInfo? dinfo, Hediff hediff, ref Pawn ___pawn)
            {
                if (Utils.IsSurrogate(___pawn))
                {
                    // Surrogates from other factions are fail-deadly, and will self-immolate to prevent capture.
                    if (___pawn.Faction != null && ___pawn.Faction != Faction.OfPlayer)
                    {
                        ___pawn.TakeDamage(new DamageInfo(DamageDefOf.Bomb, 99999f, 999f, -1f, null, ___pawn.health.hediffSet.GetBrain()));
                        return false;
                    }
                }

                if (___pawn.kindDef == ATR_PawnKindDefOf.ATR_MicroScyther || ___pawn.kindDef == ATR_PawnKindDefOf.ATR_FractalAbomination)
                {
                    ___pawn.Kill(dinfo, hediff);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        // Handle dead pawns in terms of the SkyMind network and surrogates.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
        public static class DeadSurrogatesBrain_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Pawn ___pawn)
            {
                if (!__result || !___pawn.RaceProps.Humanlike)
                    return;

                // Dead pawns always try to disconnect from the network. This only actually affects player pawns, as they are the only things actually in the network.
                Utils.gameComp.DisconnectFromSkyMind(___pawn);

                // Non-player surrogates must disconnect directly.
                if (Utils.IsSurrogate(___pawn) && ___pawn.Faction != Faction.OfPlayer)
                    ___pawn.GetComp<CompSkyMindLink>().DisconnectController();
            }
        }

        // Some notifications about player pawns dying should be suppressed, like surrogates.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]
        public class NotifyPlayerOfKilled_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(ref DamageInfo? dinfo, ref Hediff hediff, ref Caravan caravan, Pawn ___pawn)
            {
                // If the pawn is a surrogate and wasn't just turned into one, then abort.
                if (Utils.IsSurrogate(___pawn) && hediff != null && hediff.def != ATR_HediffDefOf.ATR_SkyMindReceiver)
                {
                    return false;
                }

                return true;
            }
        }
    }
}