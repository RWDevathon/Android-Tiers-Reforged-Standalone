using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ATReforged
{
    // Mod extension for races to control some features. These attributes are only used for humanlikes, there is no reason to provide any to non-humanlikes.
    public class ATR_MechTweaker : DefModExtension
    {
        // Simple bool for whether this race may be assigned as an android and drone, respectively. Disabling both would effectively mean this pawn is blacklisted from being mechanical.
        // However, disabling both will throw a config error, as having the extension will result in some other changes elsewhere, for details like corpses rotting and what category they reside in.
        public bool canBeAndroid = true;
        public bool canBeDrone = true;

        public bool isSpecialMechanical = false;

        // Simple int for the stat level of this race when set as a drone. This does nothing if the race is not considered drones.
        public int droneSkillLevel = 8;

        // Controls for what backstory the drones of this race will be set to. If the bool is false, this mod will trust the PawnKindDefs to provide correct backstories. This does nothing if the race is not considered drones.
        public bool letPawnKindHandleDroneBackstories = false; // This is likely desired to be false if a race can be either androids or drones!
        public BackstoryDef droneChildhoodBackstoryDef;
        public BackstoryDef droneAdulthoodBackstoryDef;

        // Simple bool for whether drones can have traits.
        public bool dronesCanHaveTraits = false;

        public override IEnumerable<string> ConfigErrors()
        {

            if (!canBeAndroid && !canBeDrone)
            {
                yield return "[ATR] A race was given the ATR_MechTweaker DefModExtension but had both canBeAndroid and canBeDrone set to false! This means it can not be mechanical. This extension should be removed from the race.";
            }
        }
    }
}