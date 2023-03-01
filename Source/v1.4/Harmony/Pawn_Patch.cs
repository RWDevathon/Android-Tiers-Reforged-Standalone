using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    public class Pawn_Patch
    {
        // Some things explode when they die and are destroyed instantly.
        [HarmonyPatch(typeof(Pawn), "Kill")]
        public static class Kill_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(ref Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
            {
                if (__instance.kindDef == ATR_PawnKindDefOf.ATR_MicroScyther)
                {
                    // Save details and destroy before doing the explosion to avoid the damage hitting the pawn, killing them again.
                    if (!__instance.Destroyed)
                    {
                        IntVec3 tempPos = __instance.Position;
                        Map tempMap = __instance.Map;
                        __instance.Destroy();
                        GenExplosion.DoExplosion(tempPos, tempMap, 0.3f, DamageDefOf.Bomb, __instance, 5);
                    }
                    return false;
                }
                else if (__instance.kindDef == ATR_PawnKindDefOf.ATR_FractalAbomination)
                {
                    // Save details and destroy before doing the explosion to avoid the damage hitting the pawn, killing them again.
                    if (!__instance.Destroyed)
                    {
                        IntVec3 tempPos = __instance.Position;
                        Map tempMap = __instance.Map;
                        __instance.Destroy();
                        GenExplosion.DoExplosion(tempPos, tempMap, 2, DamageDefOf.Flame, __instance, 10);
                        GenExplosion.DoExplosion(tempPos, tempMap, 0.5f, DamageDefOf.Bomb, __instance, 10, postExplosionSpawnThingDef: ATR_ThingDefOf.ATR_FractalPill, postExplosionSpawnChance: 1f, postExplosionSpawnThingCount: 1);
                    }
                    return false;
                }
                return true;
            }
        }
    }
}