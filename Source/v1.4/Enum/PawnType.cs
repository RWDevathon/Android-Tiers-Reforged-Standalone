using System;

namespace ATReforged
{
    [Flags]
    public enum PawnType
    {
        None = 0b_0000_0000,  // 0
        Drone = 0b_0000_0001,  // 1
        Android = 0b_0000_0010,  // 2
        Mechanical = Drone | Android, // 3
        Organic = 0b_0000_0100,  // 4
        NonAI = Drone | Organic, // 5
        Autonomous = Android | Organic, // 6
        All = Drone | Android | Organic // 7
    }
}
