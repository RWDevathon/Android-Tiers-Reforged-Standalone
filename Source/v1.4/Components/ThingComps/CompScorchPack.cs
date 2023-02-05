using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ATReforged
{
    public class CompScorchPack : CompAIUsablePack
    {
        protected override float ChanceToUse(Pawn wearer)
        {
            float targetValue = 0;
            int num = GenRadial.NumCellsInRadius(1.9f);
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = wearer.Position + GenRadial.RadialPattern[i];
                if (!c.InBounds(wearer.Map))
                {
                    continue;
                }
                List<Thing> thingList = c.GetThingList(wearer.Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    // If the targetValue is somehow high enough, terminate prematurely.
                    if (targetValue == 20)
                        return 1f;

                    if (thingList[j] is Pawn target && !target.HasAttachment(ThingDefOf.Fire))
                    {
                        if (target.HostileTo(wearer))
                            targetValue += 1;
                        else if (target.GetStatValue(StatDefOf.Flammability) > 0.2)
                            targetValue -= 1;
                    }
                }
            }
            return targetValue / 20;
        }

        protected override void UsePack(Pawn wearer)
        {
            Verb_FirefoamPop.Pop(wearer, parent.GetComp<CompExplosive>(), parent.GetComp<CompReloadable>());
        }
    }
}
