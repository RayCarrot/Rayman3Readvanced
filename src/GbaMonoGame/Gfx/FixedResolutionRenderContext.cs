namespace GbaMonoGame;

public class FixedResolutionRenderContext : RenderContext
{
    public FixedResolutionRenderContext(Vector2 fixedResolution)
    {
        FixedResolution = fixedResolution;
    }

    public Vector2 FixedResolution { get; }

    protected override Vector2 GetResolution()
    {
        return FixedResolution;
    }
}