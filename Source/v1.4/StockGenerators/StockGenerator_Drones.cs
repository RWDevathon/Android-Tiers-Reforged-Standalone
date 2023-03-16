using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ATReforged
{
    public class StockGenerator_Drones : StockGenerator_Slaves
    {
        protected bool respectPopulationIntent;

        protected List<PawnKindDef> droneKinds;

        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
        {
            if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
            {
                yield break;
            }

            if (!FactionHasDrones(faction))
            {
                yield break;
            }

            // Get an enumerable of all drone kinds that are actually drones, which is non-static as settings may change at any time.
            IEnumerable<PawnKindDef> usableKinds = droneKinds.Where(kindDef => Utils.IsConsideredMechanicalDrone(kindDef.race));

            for (int i = countRange.RandomInRange; i >= 0; i--)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(usableKinds.RandomElement(), null, PawnGenerationContext.NonPlayer, forTile, forceGenerateNewPawn: true);
                Pawn result = PawnGenerator.GeneratePawn(request);
                result.guest.joinStatus = JoinStatus.JoinAsColonist;
                yield return result;
            }
        }

        public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }

            if (droneKinds == null)
            {
                yield return $"StockGenerator_Drones in {trader.defName} was not supplied with any drone pawn kinds!";
            }
        }

        // Check whether the provided trader's faction is allowed to have drones to sell.
        virtual protected bool FactionHasDrones(Faction faction)
        {
            return true;
        }
    }
}
