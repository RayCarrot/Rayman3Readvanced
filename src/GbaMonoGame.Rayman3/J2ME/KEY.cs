using System;

namespace GbaMonoGame.Rayman3.J2ME;

[Flags]
public enum KEY
{
    NONE = 0,
    NUM_0 = 1 << 0,
    NUM_1 = 1 << 1,
    NUM_2 = 1 << 2,
    NUM_3 = 1 << 3,
    NUM_4 = 1 << 4,
    NUM_5 = 1 << 9,
    NUM_6 = 1 << 5,
    NUM_7 = 1 << 6,
    NUM_8 = 1 << 7,
    NUM_9 = 1 << 8,
    STAR = 1 << 10,
    POUND = 1 << 11,

    UP_ARROW = NUM_2,
    LEFT_ARROW = NUM_4,
    RIGHT_ARROW = NUM_6,
    DOWN_ARROW = NUM_8,

    SOFTKEY1 = 1 << 17,
    SOFTKEY2 = 1 << 18,
    SOFTKEY3 = NUM_5,

    MASK_START = 1 << 19,

    END = 1 << 25,
}