using RimWorld;
using Verse;
using Verse.AI;

namespace ATReforged
{
    // Attack anything that isn't the same def as this. IE. Don't attack other pawns of the same def but anything else is fair game.
    public class MentalState_Exterminator : MentalState
    { 
        public override bool ForceHostileTo(Thing t)
        {
            return t.def != pawn.def;
        }

        public override bool ForceHostileTo(Faction f)
        {
            return true;
        }

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }
}