using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class IntroCloudsRenderer : IScreenRenderer
{
    public IntroCloudsRenderer(Texture2D texture)
    {
        Texture = texture;
    }

    private const int CloudHeight = 85;

    public Texture2D Texture { get; }
    public PaletteTexture PaletteTexture { get; set; }

    private float[] GetScrollOffsets() =>
    [
        -(GameTime.ElapsedFrames / 2f % 256),
        -(255 - GameTime.ElapsedFrames / 4f % 256),
        -(GameTime.ElapsedFrames / 8f % 256)
    ];

    public Vector2 GetSize(GfxScreen screen) => new(Texture.Width, Texture.Height);
    public Box GetRenderBox(GfxScreen screen) => new(GetScrollOffsets().Min(), 0, Texture.Width, Texture.Height);

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginRender(new RenderOptions(screen.RenderContext)
        {
            Alpha = screen.IsAlphaBlendEnabled,
            PaletteTexture = PaletteTexture,
        });

        int index = 0;
        foreach (float scrollOffset in GetScrollOffsets())
        {
            Rectangle rect = new(0, CloudHeight * index, Texture.Width, CloudHeight);
            renderer.Draw(Texture, position + new Vector2(scrollOffset, CloudHeight * index), rect, color);
            
            index++;
        }
    }
}