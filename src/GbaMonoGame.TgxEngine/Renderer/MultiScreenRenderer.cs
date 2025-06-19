using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class MultiScreenRenderer : IScreenRenderer
{
    public MultiScreenRenderer(Section[] sections, Vector2 fullSize)
    {
        Sections = sections;
        FullSize = fullSize;
    }

    public Section[] Sections { get; }
    public Vector2 FullSize { get; }

    public Vector2 GetSize(GfxScreen screen) => FullSize;

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        // TODO: Only render if on screen
        foreach (Section section in Sections)
            section.ScreenRenderer.Draw(renderer, screen, position + section.Position, color);
    }

    public class Section
    {
        public Section(IScreenRenderer screenRenderer, Vector2 position)
        {
            ScreenRenderer = screenRenderer;
            Position = position;
        }

        public IScreenRenderer ScreenRenderer { get; }
        public Vector2 Position { get; }
    }
}