using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

// TODO: Would look nicer with a smoother gradient. Same for the red fog, but less noticeable there. Can create shader which interpolates vertically between two colors.
public class Mode7FogScreenEffect : ScreenEffect
{
    public Mode7FogScreenEffect(FogLine[] fogLines)
    {
        FogLines = fogLines;
    }

    public FogLine[] FogLines { get; }
    public float FadeDecrease { get; set; }
    public float Horizon { get; set; }
    public bool ShouldDraw { get; set; }

    public override void Draw(GfxRenderer renderer)
    {
        if (!ShouldDraw)
            return;

        renderer.BeginSpriteRender(RenderOptions);

        Vector2 res = RenderContext.Resolution;

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

            renderer.DrawFilledRectangle(new Vector2(0, currentScanline), new Vector2(res.X, nextScanline - currentScanline), Color.White * fade);

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