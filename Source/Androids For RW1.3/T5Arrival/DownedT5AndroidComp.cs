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
            for (int i = this.pawn.Count - 1; i >= 0; i--)
            {
                if (!this.pawn[i].Dead)
                {
                    this.pawn[i].Kill(null, null);
                }
            }
            this.pawn.ClearAndDestroyContents(DestroyMode.Vanish);
        }
        public override string CompInspectStringExtra()
        {
            if (this.pawn.Any)
            {
                return "Transcendant".Translate() + ": " + this.pawn[0].LabelShort;
            }
            return null;
        }
    }
}