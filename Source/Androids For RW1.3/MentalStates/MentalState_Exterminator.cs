using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Attack anything that isn't the same def as this. IE. Don't attack other pawns of the same race but anything else is fair game.
    public class MentalState_Exterminator : MentalState_Manhunter
    { 
        public override bool ForceHostileTo(Thing t)
        {
            return t.def != pawn.def;
        }

        public override bool ForceHostileTo(Faction f)
        {
            return true;
        }
    }
}