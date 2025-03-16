namespace GbaMonoGame;

// TODO: Add more inputs here to avoid hard-coding them, such as for pausing, speeding up the game etc.
public enum Input
{
    // GBA
    Gba_A,
    Gba_B,
    Gba_Select,
    Gba_Start,
    Gba_Right,
    Gba_Left,
    Gba_Up,
    Gba_Down,
    Gba_R,
    Gba_L,

    // Menu TODO: Remove?
    Menu_Right,
    Menu_Left,
    Menu_Up,
    Menu_Down,
    Menu_Confirm,
    Menu_Back,

    // Debug
    Debug_ToggleBoxes,
    Debug_ToggleCollision,
    Debug_ToggleNoClip,
}