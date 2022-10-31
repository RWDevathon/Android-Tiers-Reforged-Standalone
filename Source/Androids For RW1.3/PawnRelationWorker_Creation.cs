using System.Linq;
using RimWorld;
using Verse;

namespace ATReforged
{
    public class PawnRelationWorker_Creation : PawnRelationWorker
    {
            public override bool InRelation(Pawn me, Pawn other)
            {
                Log.Error("Why is this InRelation() from PawnRelationWorker_Creation used? All it does is return whether two pawns are the same");
                return me != other;
            }
    }
}
