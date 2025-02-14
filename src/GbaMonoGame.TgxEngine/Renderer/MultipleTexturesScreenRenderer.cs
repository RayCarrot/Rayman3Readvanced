using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

public class MultipleTexturesScreenRenderer : IScreenRenderer
{
    public MultipleTexturesScreenRenderer(Texture2D[] textures)
    {
        Textures = textures;
        SelectedTexture = 0;
        TextureRectangle = textures[SelectedTexture].Bounds;
    }

    public Texture2D[] Textures { get; }
    public int SelectedTexture { get; set; }
    public Rectangle TextureRectangle { get; set; }

    public Vector2 GetSize(GfxScreen screen) => TextureRectangle.Size.ToVector2();

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginRender(screen.RenderOptions);

        renderer.Draw(Textures[SelectedTexture], position, TextureRectangle, color);
    }
}