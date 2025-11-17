using System;

namespace GbaMonoGame;

[Flags]
public enum Input
{
    // GBA
    Gba_A = 1 << 0, // N-Gage: 5
    Gba_B = 1 << 1, // N-Gage: 7
    Gba_Select = 1 << 2, // N-Gage: Right soft key
    Gba_Start = 1 << 3, // N-Gage: Left soft key
    Gba_Right = 1 << 4,
    Gba_Left = 1 << 5,
    Gba_Up = 1 << 6,
    Gba_Down = 1 << 7,
    Gba_R = 1 << 8, // N-Gage: 8
    Gba_L = 1 << 9, // N-Gage: 6

    // Debug
    Debug_ToggleDebugMode = 1 << 10,
    Debug_TogglePause = 1 << 11,
    Debug_StepOneFrame = 1 << 12,
    Debug_SpeedUp = 1 << 13,
    Debug_ToggleDisplayBoxes = 1 << 14,
    Debug_ToggleDisplayCollision = 1 << 15,
    Debug_ToggleNoClip = 1 << 16,
}