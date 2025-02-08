using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class TextLayerRenderContext : RenderContext
{
    public TextLayerRenderContext(RenderContext parentRenderContext)
    {
        ParentRenderContext = parentRenderContext;
    }

    private Vector2 _lastParentResolution;

    // Do not center as we want it to render on the top of the screen
    protected override bool Center => false;

    public RenderContext ParentRenderContext { get; }
    public float Horizon { get; set; }

    protected override Vector2 GetResolution()
    {
        _lastParentResolution = ParentRenderContext.Resolution;
        return ParentRenderContext.Resolution with{ Y = Horizon + 1 };
    }

    public override void Update()
    {
        base.Update();

        if (_lastParentResolution != ParentRenderContext.Resolution)
            UpdateResolution();
    }
}