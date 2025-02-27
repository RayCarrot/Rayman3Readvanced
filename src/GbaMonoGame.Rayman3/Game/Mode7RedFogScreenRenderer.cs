using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class Mode7RedFogScreenRenderer : IScreenRenderer
{
    public Mode7RedFogScreenRenderer(FogLine[] fogLines)
    {
        FogLines = fogLines;
    }

    public FogLine[] FogLines { get; }
    public int ColorAdd { get; set; }

    public Vector2 GetSize(GfxScreen screen)
    {
        return screen.RenderOptions.RenderContext.Resolution;
    }

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginRender(screen.RenderOptions);

        Vector2 res = screen.RenderOptions.RenderContext.Resolution;

        float currentScanline = 0;
        for (int i = 0; i < FogLines.Length; i++)
        {
            FogLine fogLine = FogLines[i];
            
            float nextScanline = i + 1 == FogLines.Length ? res.Y : FogLines[i + 1].Scanline;

            color = new Color(ColorHelpers.FromRGB555(fogLine.Color + ColorAdd), color.A);
            renderer.DrawFilledRectangle(new Vector2(0, currentScanline), new Vector2(res.X, nextScanline - currentScanline), color);

            currentScanline = nextScanline;
        }
    }

    public class FogLine
    {
        public FogLine(int color, int scanline)
        {
            Color = color;
            Scanline = scanline;
        }

        public int Color { get; }
        public int Scanline { get; }
    }
}