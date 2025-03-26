namespace GbaMonoGame.Engine2d;

public class HudRenderContext : RenderContext
{
    public HudRenderContext(RenderContext sceneRenderContext)
    {
        SceneRenderContext = sceneRenderContext;
    }

    public RenderContext SceneRenderContext { get; }

    protected override Vector2 GetResolution()
    {
        // Internal game resolution, but with the aspect ratio used in the scene
        return Engine.GameRenderContext.Resolution.ShrinkToAspectRatio(SceneRenderContext.Resolution);
    }
}