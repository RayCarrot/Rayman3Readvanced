namespace GbaMonoGame;

public class ViewPortManager
{
    public ViewPortManager()
    {
        GameRenderContext = new GameRenderContext();
    }

    /// <summary>
    /// The primary render context using the internal game resolution
    /// </summary>
    public GameRenderContext GameRenderContext { get; }

    /// <summary>
    /// The internal game resolution used for the aspect ratio and scaling
    /// </summary>
    public Vector2 InternalGameResolution { get; private set; }

    public Vector2 FullSize { get; private set; }
    public Box RenderBox { get; private set; }

    public void SetInternalGameResolution(Vector2 resolution)
    {
        InternalGameResolution = resolution;
        UpdateRenderBox();
    }

    public void Resize(Vector2 newScreenSize)
    {
        FullSize = newScreenSize;
        UpdateRenderBox();
    }

    public void UpdateRenderBox()
    {
        Vector2 scaledScreenSize = FullSize.ShrinkToAspectRatio(InternalGameResolution);

        RenderBox = new Box(
            position: (FullSize - scaledScreenSize) / 2,
            size: scaledScreenSize);
    }
}