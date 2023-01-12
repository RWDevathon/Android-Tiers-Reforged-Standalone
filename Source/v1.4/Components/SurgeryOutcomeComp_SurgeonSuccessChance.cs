using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ATReforged
{
    // Override AffectQuality to use ATR_MechanicalSurgerySuccessChance instead of SurgerySuccessChance
    public class SurgeryOutcomeComp_MechanicSuccessChance : SurgeryOutcomeComp_SurgeonSuccessChance
    {
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            quality *= surgeon.GetStatValue(ATR_StatDefOf.ATR_MechanicalSurgerySuccessChance);
        }
    }
}