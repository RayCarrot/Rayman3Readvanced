using System;

namespace GbaMonoGame.Rayman3.J2ME;

[Flags]
public enum GAME_KEY : ushort
{
    NONE = 0,
    ZERO = 1 << 0,
    UP_LEFT = 1 << 1,
    UP = 1 << 2,
    UP_RIGHT = 1 << 3,
    LEFT = 1 << 4,
    MIDDLE = 1 << 5,
    RIGHT = 1 << 6,
    DOWN_LEFT = 1 << 7,
    DOWN = 1 << 8,
    DOWN_RIGHT = 1 << 9,
    STAR = 1 << 10, // Unused
    POUND = 1 << 11, // Unused
    SOFTKEY_1 = 1 << 12,
    SOFTKEY_2 = 1 << 13,

    END = 1 << 15, // Unused
}