using System;

namespace GbaMonoGame.Rayman3.J2ME;

[Flags]
public enum ANIM_DATA_FLAGS : sbyte
{
    NONE = 0,
    HAS_MECH_MODEL = 1 << 0,
    LOADED = 1 << 1,
}