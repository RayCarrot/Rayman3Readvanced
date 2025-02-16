using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class TextLayerRenderContext : RenderContext
{
    public TextLayerRenderContext(RenderContext parentRenderContext)
    {
        ParentRenderContext = parentRenderContext;
    }

    private Vector2 _lastParentResolution;

    // Anchor to the top instead of centering
    protected override VerticalAlignment VerticalAlignment => VerticalAlignment.Top;

    public RenderContext ParentRenderContext { get; }
    public float Horizon { get; set; }

    protected override Vector2 GetResolution()
    {
        _lastParentResolution = ParentRenderContext.Resolution;
        return ParentRenderContext.Resolution with { Y = Horizon + 1 };
    }

    public override void Update()
    {
        base.Update();

        if (_lastParentResolution != ParentRenderContext.Resolution)
            UpdateResolution();
    }
}