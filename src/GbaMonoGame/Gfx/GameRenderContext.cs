namespace GbaMonoGame;

/// <summary>
/// The primary render context using the current game resolution
/// </summary>
public sealed class GameRenderContext : RenderContext
{
    protected override Vector2 GetResolution() => Engine.GameViewPort.GameResolution;
}