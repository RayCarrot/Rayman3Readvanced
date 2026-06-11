using System;

namespace GbaMonoGame.Rayman3.J2me;

[Flags]
public enum ANIM_EVENT_FLAGS
{
    NONE = 0,
    LOADED_MECH_MODEL = 1 << 0,
    LOADED_COLLISION_BOX = 1 << 1,
}