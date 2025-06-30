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
        // Only draw on-screen sections if in 2D
        if (screen.RenderOptions.WorldViewProj == null)
        {
            Box renderBox = new(Vector2.Zero, screen.RenderOptions.RenderContext.Resolution);

            foreach (Section section in Sections)
            {
                Box sectionRenderBox = section.ScreenRenderer.GetRenderBox(screen);
                sectionRenderBox = Box.Offset(sectionRenderBox, position + section.Position);

                // Check if on screen
                if (renderBox.Intersects(sectionRenderBox))
                    section.ScreenRenderer.Draw(renderer, screen, position + section.Position, color);
            }
        }
        else
        {
            foreach (Section section in Sections)
                section.ScreenRenderer.Draw(renderer, screen, position + section.Position, color);
        }
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