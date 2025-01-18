namespace GbaMonoGame;

/// <summary>
/// The render context for the original game resolution with the internal resolution's aspect ratio
/// </summary>
public sealed class OriginalScaledGameRenderContext : RenderContext
{
    protected override Vector2 GetResolution()
    {
        return Rom.OriginalResolution.ExtendToAspectRatio(Engine.Config.InternalGameResolution);
    }
}