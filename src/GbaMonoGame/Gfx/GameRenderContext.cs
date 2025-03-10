namespace GbaMonoGame;

/// <summary>
/// The primary render context using the internal game resolution
/// </summary>
public sealed class GameRenderContext : RenderContext
{
    protected override Vector2 GetResolution() => Engine.InternalGameResolution;
}