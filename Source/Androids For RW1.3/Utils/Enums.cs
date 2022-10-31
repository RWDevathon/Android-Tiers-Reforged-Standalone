using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATReforged
{
    public static class Enums
    { // If it's a button, it should have an enumeration!
        public enum OptionsTab { General, Power, Security, Health, Connectivity, Stats }
        public enum SettingsPreset { None, Default, Custom } //, DigitalWarfare, NoConnections, MetalSuperiority, FleshSuperiority }
        public enum PawnListKind { None, Android, Drone, Animal }

    }
}
