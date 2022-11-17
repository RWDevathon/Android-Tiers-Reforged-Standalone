using Verse;
using RimWorld.Planet;

namespace ATReforged
{
    public class DownedT5AndroidComp : DownedRefugeeComp
    {
        protected override string PawnSaveKey
        {
            get
            {
                return "enigmatic archotech";
            }
        }

        protected override void RemovePawnOnWorldObjectRemoved()
        {
            pawn.ClearAndDestroyContents(DestroyMode.Vanish);
        }

        public override string CompInspectStringExtra()
        {
            return null;
        }
    }
}