using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

// For now the width is hard-coded to 256 since the map for some reason has a lot of empty space to the right and
// we do wrapping based on the entire map rather than what's loaded into vram (like the gba does).
public class LevelCloudsRenderer : IScreenRenderer
{
    public LevelCloudsRenderer(Texture2D texture, int[] splits)
    {
        Texture = texture;
        Splits = splits;

        TextureWidth = 256;
        TextureHeight = texture.Height;
    }

    public Texture2D Texture { get; }
    private int[] Splits { get; }
    private int TextureWidth { get; }
    private int TextureHeight { get; }

    private float[] GetScrollOffsets() =>
    [
        -(GameTime.ElapsedFrames / 2f % 256),
        -(GameTime.ElapsedFrames / 4f % 256),
        -(GameTime.ElapsedFrames / 8f % 256),
    ];

    public Vector2 GetSize(GfxScreen screen) => new(TextureWidth, TextureHeight);
    public Box GetRenderBox(GfxScreen screen) => new(GetScrollOffsets().Min(), 0, TextureWidth, TextureHeight);

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginSpriteRender(screen.RenderOptions);

        int offsetY = 0;
        int index = 0;
        foreach (float scrollOffset in GetScrollOffsets())
        {
            Rectangle rect = new(0, offsetY, TextureWidth, Splits[index] - offsetY);
            renderer.Draw(Texture, position + new Vector2(scrollOffset, offsetY), rect, color);

            offsetY = Splits[index];
            index++;
        }
    }
}