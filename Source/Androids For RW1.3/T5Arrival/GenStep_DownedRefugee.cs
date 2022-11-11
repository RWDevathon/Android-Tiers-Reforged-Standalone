using Verse;

namespace ATReforged
{
    public class GenStep_DownedT5Android : GenStep_Scatterer
    {
        public override int SeedPart
        {
            get
            {
                return 931842770;
            }
        }
        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            return base.CanScatterAt(c, map) && c.Standable(map);
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            DownedT5AndroidComp component = map.info.parent.GetComponent<DownedT5AndroidComp>();
            Pawn newThing;
            if (component != null && component.pawn.Any)
            {
                newThing = component.pawn.Take(component.pawn[0]);
            }
            else
            {
                newThing = DownedT5Utility.GenerateT5(map.Tile);
            }
            GenSpawn.Spawn(newThing, loc, map);
            MapGenerator.rootsToUnfog.Add(loc);
        }
    }
}