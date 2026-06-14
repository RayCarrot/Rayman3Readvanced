using System;

namespace GbaMonoGame.Rayman3;

// TODO: These don't work well with audio panning since only one plays at a time
[Flags]
public enum ActorSoundFlags
{
    None = 0,
    LavaFall = 1 << 0,
    FlyingBomb = 1 << 1,
    Electricity = 1 << 2,
    Urchin = 1 << 3,
}