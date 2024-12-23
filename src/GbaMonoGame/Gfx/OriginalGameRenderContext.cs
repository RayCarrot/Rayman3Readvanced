namespace GbaMonoGame;

/// <summary>
/// The render context for the original game resolution with the screen aspect ratio
/// </summary>
public sealed class OriginalGameRenderContext : RenderContext
{
    protected override Vector2 GetResolution()
    {
        float ratio = Engine.GameViewPort.GameResolution.X / Engine.GameViewPort.GameResolution.Y;

        if (ratio > 1)
            return new Vector2(ratio * Engine.GameViewPort.OriginalGameResolution.Y, Engine.GameViewPort.OriginalGameResolution.Y);
        else
            return new Vector2(Engine.GameViewPort.OriginalGameResolution.X, 1 / ratio * Engine.GameViewPort.OriginalGameResolution.X);
    }
}