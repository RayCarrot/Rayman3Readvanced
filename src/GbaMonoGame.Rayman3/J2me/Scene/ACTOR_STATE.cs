using System;

namespace GbaMonoGame.Rayman3.J2me;

[Flags]
public enum ACTOR_STATE : short
{
    NONE = 0,
    FLIP_X = 1 << 0,
    LEFT_TO_DIE = 1 << 1,
    FLIP_Y = 1 << 2,
    DEAD = 1 << 3,
    USE_MECH_MODEL = 1 << 4,
    MORPH_ON_DEATH = 1 << 5,
    MORPHED = 1 << 6,
}