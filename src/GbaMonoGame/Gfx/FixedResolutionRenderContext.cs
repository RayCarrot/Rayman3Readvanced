namespace GbaMonoGame;

public class FixedResolutionRenderContext : RenderContext
{
    public FixedResolutionRenderContext(
        Vector2 fixedResolution, 
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, 
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        bool fitToGameViewPort = true)
    {
        FixedResolution = fixedResolution;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
        FitToGameViewPort = fitToGameViewPort;
    }

    protected override HorizontalAlignment HorizontalAlignment { get; }
    protected override VerticalAlignment VerticalAlignment { get; }
    protected override bool FitToGameViewPort { get; }

    public Vector2 FixedResolution { get; private set; }

    protected override Vector2 GetResolution()
    {
        return FixedResolution;
    }

    public void SetResolution(Vector2 resolution)
    {
        FixedResolution = resolution;
        UpdateResolution();
    }
}