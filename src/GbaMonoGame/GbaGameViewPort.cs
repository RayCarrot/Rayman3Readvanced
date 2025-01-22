namespace GbaMonoGame;

public class GbaGameViewPort
{
    public Vector2 FullSize { get; private set; }
    public Box RenderBox { get; private set; }

    public void Resize(Vector2 newScreenSize)
    {
        FullSize = newScreenSize;
        UpdateRenderBox();
    }

    public void UpdateRenderBox()
    {
        Vector2 scaledScreenSize = FullSize.ShrinkToAspectRatio(Engine.Config.InternalGameResolution);

        RenderBox = new Box(
            position: (FullSize - scaledScreenSize) / 2,
            size: scaledScreenSize);
    }
}