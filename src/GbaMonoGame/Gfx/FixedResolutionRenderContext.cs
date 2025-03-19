namespace GbaMonoGame;

public class FixedResolutionRenderContext : RenderContext
{
    public FixedResolutionRenderContext(
        Vector2 fixedResolution, 
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, 
        VerticalAlignment verticalAlignment = VerticalAlignment.Center)
    {
        FixedResolution = fixedResolution;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
    }

    protected override HorizontalAlignment HorizontalAlignment { get; }
    protected override VerticalAlignment VerticalAlignment { get; }

    public Vector2 FixedResolution { get; }

    protected override Vector2 GetResolution()
    {
        return FixedResolution;
    }
}