using Verse;
using HarmonyLib;
using RimWorld;

namespace ATReforged
{
    internal class DownedRefugeeQuestUtility_Patch
    {
        // Refugees that are members of an android faction must be androids.
        [HarmonyPatch(typeof(DownedRefugeeQuestUtility), "GenerateRefugee")]
        public class GenerateRefugee_Patch
        {
            [HarmonyPostfix]
            public static void Listener(int tile, ref Pawn __result, PawnKindDef pawnKind = null, float chanceForFaction = 0.6f)
            {
                if (__result != null && __result.Faction != null && Utils.ReservedAndroidFactions.Contains(__result.Faction.def.defName) && !Utils.IsConsideredMechanical(__result))
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(__result.Faction.def.basicMemberKind, __result.Faction, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: true, canGeneratePawnRelations: false, allowFood: true));
                    HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
                    HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, allowBleedingWounds: false);
                    __result = pawn;
                }
            }
        }
    }
}