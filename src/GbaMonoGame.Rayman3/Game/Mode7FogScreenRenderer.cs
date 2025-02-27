using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class Mode7FogScreenRenderer : IScreenRenderer
{
    public Mode7FogScreenRenderer(FogLine[] fogLines)
    {
        FogLines = fogLines;
    }

    public FogLine[] FogLines { get; }
    public float FadeDecrease { get; set; }
    public float Horizon { get; set; }

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
            
            float nextScanline = i + 1 == FogLines.Length ? res.Y : Horizon + FogLines[i + 1].RelativeScanline;

            float fade = fogLine.Fade - FadeDecrease;

            if (fade < 0)
                fade = 0;

            // Convert to 0-1 range from GBA's 0-16 range
            fade /= 16;

            color = Color.White * fade;
            renderer.DrawFilledRectangle(new Vector2(0, currentScanline), new Vector2(res.X, nextScanline - currentScanline), color);

            currentScanline = nextScanline;
        }
    }

    public class FogLine
    {
        public FogLine(int fade, int relativeScanline)
        {
            Fade = fade;
            RelativeScanline = relativeScanline;
        }

        public int Fade { get; }
        public int RelativeScanline { get; }
    }
}