namespace GbaMonoGame;

public enum SpriteType
{
    /// <summary>
    /// Normal sprites.
    /// </summary>
    Default,

    /// <summary>
    /// Sprites which are added in last. This is needed because the game sometimes adds
    /// sprites at the end of OAM to make sure they get a different priority.
    /// </summary>
    Back,

    /// <summary>
    /// Sprites which are to be rendered on top of everything else in the screen.
    /// </summary>
    Overlay,
}