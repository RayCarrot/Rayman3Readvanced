using System;

namespace GbaMonoGame.Rayman3.J2me;

[Flags]
public enum RESOURCE_STATUS : sbyte
{
    NONE = 0,
    REQUESTED = 1 << 0,
    LOADED = 1 << 1,
    USED = 1 << 2,
}