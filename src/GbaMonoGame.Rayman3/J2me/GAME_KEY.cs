using System;

namespace GbaMonoGame.Rayman3.J2me;

[Flags]
public enum GAME_KEY : ushort
{
    NONE = 0,
    UNUSED_ZERO = 1 << 0, // Unused
    UP_LEFT = 1 << 1,
    UP = 1 << 2,
    UP_RIGHT = 1 << 3,
    LEFT = 1 << 4,
    ACTION = 1 << 5,
    RIGHT = 1 << 6,
    DOWN_LEFT = 1 << 7,
    DOWN = 1 << 8,
    DOWN_RIGHT = 1 << 9,
    DEBUG = 1 << 10, // Unused, except for Readvanced cheat menu
    UNUSED_POUND = 1 << 11, // Unused
    CONFIRM_NO = 1 << 12,
    CONFIRM_YES = 1 << 13,
    PAUSE = 1 << 14, // Custom in Readvanced
    UNUSED_END = 1 << 15, // Unused
}