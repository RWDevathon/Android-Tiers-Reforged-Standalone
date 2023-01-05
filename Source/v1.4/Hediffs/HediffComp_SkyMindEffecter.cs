using Verse;

namespace ATReforged
{
    public class HediffComp_SkyMindEffecter : HediffComp
    {
        public HediffCompProperties_SkyMindEffecter Props => (HediffCompProperties_SkyMindEffecter)props;

        // This HediffComp exists only to mark a Hediff as something that affects the pawn's relation to the SkyMind network.

        public bool BlocksSkyMindConnection => Props.blocksConnection;

        public bool AllowsSkyMindConnection => Props.allowsConnection;

        public bool IsTransceiver => Props.isTransceiver;

        public bool IsReceiver => Props.isReceiver;
    }
}
