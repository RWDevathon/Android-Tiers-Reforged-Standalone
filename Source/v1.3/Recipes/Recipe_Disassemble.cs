using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Androids
{
	// Token: 0x02000039 RID: 57
	public class Recipe_Disassemble : RecipeWorker
	{
		// Token: 0x060000EF RID: 239 RVA: 0x00009868 File Offset: 0x00007A68
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			bool flag = pawn.def.HasModExtension<MechanicalPawnProperties>();
			if (flag)
			{
				yield return null;
			}
			yield break;
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00009888 File Offset: 0x00007A88
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			Need_Energy need_Energy = pawn.needs.TryGetNeed<Need_Energy>();
			EnergyTrackerComp energyTrackerComp = pawn.TryGetComp<EnergyTrackerComp>();
			bool flag = need_Energy != null;
			if (flag)
			{
				need_Energy.CurLevelPercentage = 0f;
			}
			bool flag2 = energyTrackerComp != null;
			if (flag2)
			{
				energyTrackerComp.energy = 0f;
			}
			ButcherUtility.SpawnDrops(pawn, pawn.Position, pawn.Map);
			pawn.Kill(null, null);
		}
	}
}
