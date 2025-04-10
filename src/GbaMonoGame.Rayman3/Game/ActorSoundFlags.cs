using System;

namespace GbaMonoGame.Rayman3;

[Flags]
public enum ActorSoundFlags
{
    None = 0,
    LavaFall = 1 << 0,
    FlyingBomb = 1 << 1,
    Electricity = 1 << 2,
    Urchin = 1 << 3,
}