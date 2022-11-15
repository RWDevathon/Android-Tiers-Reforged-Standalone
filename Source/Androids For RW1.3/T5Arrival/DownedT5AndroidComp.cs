using Verse;
using RimWorld.Planet;

namespace ATReforged
{
    public class DownedT5AndroidComp : ImportantPawnComp, IThingHolder
    {
        protected override string PawnSaveKey
        {
            get
            {
                return "transcendant";
            }
        }
        protected override void RemovePawnOnWorldObjectRemoved()
        {
            for (int i = pawn.Count - 1; i >= 0; i--)
            {
                if (!pawn[i].Dead)
                {
                    pawn[i].Kill(null);
                }
            }
            pawn.ClearAndDestroyContents(DestroyMode.Vanish);
        }
        public override string CompInspectStringExtra()
        {
            if (pawn.Any)
            {
                return "Transcendant".Translate() + ": " + pawn[0].LabelShort;
            }
            return null;
        }
    }
}