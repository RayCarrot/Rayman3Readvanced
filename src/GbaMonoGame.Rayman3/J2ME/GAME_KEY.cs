using System;

namespace GbaMonoGame.Rayman3.J2ME;

[Flags]
public enum GAME_KEY : ushort
{
    None = 0,
    Zero = 1 << 0,
    UpLeft = 1 << 1,
    Up = 1 << 2,
    UpRight = 1 << 3,
    Left = 1 << 4,
    Middle = 1 << 5,
    Right = 1 << 6,
    DownLeft = 1 << 7,
    Down = 1 << 8,
    DownRight = 1 << 9,
    Star = 1 << 10, // Unused
    Pound = 1 << 11, // Unused
    Softkey1 = 1 << 12,
    Softkey2 = 1 << 13,

    End = 1 << 15, // Unused
}