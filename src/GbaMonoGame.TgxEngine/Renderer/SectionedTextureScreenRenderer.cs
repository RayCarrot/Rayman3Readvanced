using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

public class SectionedTextureScreenRenderer : IScreenRenderer
{
    public SectionedTextureScreenRenderer(TextureSection[] sections, Vector2 fullSize)
    {
        Sections = sections;
        FullSize = fullSize;
    }

    public TextureSection[] Sections { get; }
    public Vector2 FullSize { get; }

    public Vector2 GetSize(GfxScreen screen) => FullSize;

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginRender(screen.RenderOptions);

        foreach (TextureSection section in Sections)
            renderer.Draw(section.Texture, position + section.Position, color);
    }

    public class TextureSection
    {
        public TextureSection(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            Position = position;
        }

        public Texture2D Texture { get; }
        public Vector2 Position { get; }
    }
}