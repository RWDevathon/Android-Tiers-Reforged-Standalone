using System.Collections.Generic;
using Verse;

namespace ATReforged
{
    public class HediffCompProperties_SkyMindEffecter : HediffCompProperties
    {
        public HediffCompProperties_SkyMindEffecter()
        {
            compClass = typeof(HediffComp_SkyMindEffecter);
        }

        // Marks the host hediff as something that allows connecting to the SkyMind network.
        public bool allowsConnection = false;

        // Marks the host hediff as something that prevents connecting to the SkyMind network.
        public bool blocksConnection = false;

        // Marks the host hediff as acting as a transceiver, meaning its host can be a surrogate controller and is not a surrogate.
        public bool isTransceiver = false;

        // Marks the host hediff as acting as a receiver, meaning its host is a surrogate. It can't be a controller.
        public bool isReceiver = false;

        // Some states are illegal and should not be allowed.
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            if (allowsConnection)
            {
                if (blocksConnection)
                {
                    yield return "HediffDef " + parentDef + " has a HediffCompProperties_SkyMindEffecter with both allowing and blocking SkyMind connections.";
                }
                if (!(isTransceiver || isReceiver))
                {
                    yield return "HediffDef " + parentDef + " has a HediffCompProperties_SkyMindEffecter that allows SkyMind connection but is neither a transceiver nor receiver.";
                }
            }

            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
        }
    }
}
