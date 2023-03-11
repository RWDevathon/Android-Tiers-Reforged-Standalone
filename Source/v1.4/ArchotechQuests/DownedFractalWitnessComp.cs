using Verse;
using RimWorld.Planet;

namespace ATReforged
{
    public class DownedFractalWitnessComp : DownedRefugeeComp
    {
        protected override string PawnSaveKey
        {
            get
            {
                return "fractal witness";
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