using System;

namespace GbaMonoGame.Rayman3.J2me;

[Flags]
public enum KEY
{
    NONE = 0,
    NUM_0 = 1 << 0, // Menu: down    |
    NUM_1 = 1 << 1, // Menu: up      | Game: up-left
    NUM_2 = 1 << 2, // Menu: up      | Game: up
    NUM_3 = 1 << 3, // Menu: up      | Game: up-right 
    NUM_4 = 1 << 4, // Menu: up      | Game: left
    NUM_5 = 1 << 9, // Menu: confirm | Game: action
    NUM_6 = 1 << 5, // Menu: down    | Game: right
    NUM_7 = 1 << 6, // Menu: down    | Game: down-left
    NUM_8 = 1 << 7, // Menu: down    | Game: down
    NUM_9 = 1 << 8, // Menu: down    | Game: down-right
    STAR = 1 << 10, //               | Game: debug
    POUND = 1 << 11,

    UP_ARROW = NUM_2,
    LEFT_ARROW = NUM_4,
    RIGHT_ARROW = NUM_6,
    DOWN_ARROW = NUM_8,

    SOFTKEY1 = 1 << 17, // Menu: confirm | Game: confirm no, pause
    SOFTKEY2 = 1 << 18, // Menu: confirm | Game: confirm yes, pause
    SOFTKEY3 = NUM_5,

    MASK_START = 1 << 19,

    END = 1 << 25,
}