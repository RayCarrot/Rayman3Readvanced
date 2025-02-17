using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class MultiSelectableScreenRenderer : IScreenRenderer
{
    public MultiSelectableScreenRenderer(IScreenRenderer[] screenRenderers)
    {
        ScreenRenderers = screenRenderers;
        SelectedScreenRenderer = 0;
    }

    public IScreenRenderer[] ScreenRenderers { get; }
    public int SelectedScreenRenderer { get; set; }

    public Vector2 GetSize(GfxScreen screen) => ScreenRenderers[SelectedScreenRenderer].GetSize(screen);

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        ScreenRenderers[SelectedScreenRenderer].Draw(renderer, screen, position, color);
    }
}