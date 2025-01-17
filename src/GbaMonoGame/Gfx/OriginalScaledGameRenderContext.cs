namespace GbaMonoGame;

/// <summary>
/// The render context for the original game resolution with the internal resolution's aspect ratio
/// </summary>
public sealed class OriginalScaledGameRenderContext : RenderContext
{
    protected override Vector2 GetResolution()
    {
        Vector2 gameResolution = Engine.Config.InternalGameResolution;
        Vector2 originalResolution = Rom.OriginalResolution;

        float ratio = gameResolution.X / gameResolution.Y;

        if (ratio > 1)
            return new Vector2(ratio * originalResolution.Y, originalResolution.Y);
        else
            return new Vector2(originalResolution.X, 1 / ratio * originalResolution.X);
    }
}