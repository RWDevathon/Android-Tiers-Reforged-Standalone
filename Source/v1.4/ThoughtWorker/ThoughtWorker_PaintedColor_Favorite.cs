using RimWorld;
using Verse;

namespace ATReforged
{
    public class ThoughtWorker_PaintedColor_Favorite : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.story?.favoriteColor.HasValue == true && Utils.IsConsideredMechanicalAndroid(p))
            {
                return p.story?.favoriteColor.Value == p.story?.SkinColorBase;
            }
            return false;
        }
    }
}
