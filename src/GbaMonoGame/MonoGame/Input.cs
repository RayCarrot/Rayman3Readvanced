namespace GbaMonoGame;

public enum Input
{
    // GBA
    Gba_A, // N-Gage: 5
    Gba_B, // N-Gage: 7
    Gba_Select, // N-Gage: Right soft key
    Gba_Start, // N-Gage: Left soft key
    Gba_Right,
    Gba_Left,
    Gba_Up,
    Gba_Down,
    Gba_R, // N-Gage: 8
    Gba_L, // N-Gage: 6

    // Debug
    Debug_ToggleDebugMode,
    Debug_TogglePause,
    Debug_StepOneFrame,
    Debug_SpeedUp,
    Debug_ToggleDisplayBoxes,
    Debug_ToggleDisplayCollision,
    Debug_ToggleNoClip,
}