using Verse;

namespace ATReforged
{
    public class CompProperties_ChargingStation : CompProperties
    {
        public CompProperties_ChargingStation()
        {
            compClass = typeof(CompChargingStation);
        }

        public int SkyMindSlotsProvided;
    }
}
