using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class SineWaveRenderer : IScreenRenderer
{
    public SineWaveRenderer(Texture2D texture)
    {
        Texture = texture;

        Lines = new Rectangle[texture.Height];
        for (int i = 0; i < Lines.Length; i++)
            Lines[i] = new Rectangle(0, i, texture.Width, 1);
    }

    public Texture2D Texture { get; }
    public PaletteTexture PaletteTexture { get; set; }
    public Rectangle[] Lines { get; }

    public float Phase { get; set; }
    public float Amplitude { get; set; }

    public Vector2 GetSize(GfxScreen screen) => new(Texture.Width, Texture.Height);
    public Box GetRenderBox(GfxScreen screen)
    {
        float maxStartX = 0;

        float phase = Phase;
        for (int i = 0; i < Lines.Length; i++)
        {
            float startX = MathHelpers.Sin256(phase) * Amplitude;

            if (startX > maxStartX)
                maxStartX = startX;
            
            phase++;
        }

        // The skull in the Cave of Bad Dreams is only on the bottom-right of the map, so we can ignore MinX
        return new Box(0, 0, maxStartX + Texture.Width, Texture.Height);
    }

    // TODO: Can be optimized. First x number of lines are blank. And also we could add culling for off-screen lines.
    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginRender(new RenderOptions(screen.RenderContext)
        {
            Alpha = screen.IsAlphaBlendEnabled,
            PaletteTexture = PaletteTexture,
        });

        float phase = Phase;
        for (int i = 0; i < Lines.Length; i++)
        {
            renderer.Draw(Texture, position + new Vector2(MathHelpers.Sin256(phase) * Amplitude, i), Lines[i], color);
            phase++;
        }
    }
}