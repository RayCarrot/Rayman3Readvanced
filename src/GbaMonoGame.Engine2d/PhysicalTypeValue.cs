﻿namespace GbaMonoGame.Engine2d;

// Types 0-31 are reserved for the engine, with 32+ being for the game. Ideally we shouldn't be including the Rayman 3 types
// in the base library here, but it's more convenient to have them all in the same enum rather than doing something like constant fields.
public enum PhysicalTypeValue : byte
{
    // Fully solid
    Solid = 0,
    Slide = 1,
    Grab = 2,
    GrabSlide = 3,
    Passthrough = 15,

    // Angled solid
    SolidAngle90Left = 16, // Unused
    SolidAngle90Right = 17, // Unused
    SolidAngle30Left1 = 18,
    SolidAngle30Left2 = 19,
    SolidAngle30Right1 = 20,
    SolidAngle30Right2 = 21,
    SlideAngle30Left1 = 22,
    SlideAngle30Left2 = 23,
    SlideAngle30Right1 = 24,
    SlideAngle30Right2 = 25,

    // Rayman 3
    InstaKill = 32,
    Damage = 33,
    Enemy_Left = 34,
    Enemy_Right = 35,
    Enemy_Up = 36,
    Enemy_Down = 37,

    MovingPlatform_FullStop = 36,
    MovingPlatform_Stop = 37,
    MovingPlatform_Left = 38,
    MovingPlatform_Right = 39,
    MovingPlatform_Up = 40,
    MovingPlatform_Down = 41,
    MovingPlatform_DownLeft = 42,
    MovingPlatform_DownRight = 43,
    MovingPlatform_UpRight = 44,
    MovingPlatform_UpLeft = 45,
    Hang = 46,
    Climb = 47,
    Water = 48,
    WallJump = 49,
    SlideJump = 50,
    Spider_Right = 51,
    Spider_Left = 52,
    Spider_Up = 53,
    Spider_Down = 54,

    Lava = 74,

    MovingPlatform_CounterClockwise45 = 81,
    MovingPlatform_CounterClockwise90 = 82,
    MovingPlatform_CounterClockwise135 = 83, // Unused NOTE: Does not currently have graphics in the collision tile set
    MovingPlatform_CounterClockwise180 = 84, // Unused NOTE: Does not currently have graphics in the collision tile set
    MovingPlatform_CounterClockwise225 = 85, // Unused NOTE: Does not currently have graphics in the collision tile set
    MovingPlatform_CounterClockwise270 = 86,
    MovingPlatform_CounterClockwise315 = 87,

    MoltenLava = 90,

    None = 0xFF,
}