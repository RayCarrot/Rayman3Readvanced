namespace GbaMonoGame;

// TODO: Add more inputs here to avoid hard-coding them, such as for pausing, speeding up the game etc.
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
    Debug_ToggleBoxes,
    Debug_ToggleCollision,
    Debug_ToggleNoClip,
}