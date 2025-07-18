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
        // TRAILER
        float scale = Engine.InternalGameResolution.X / Resolution.Modern.X;
        Vector2 scaledScreenSize = FullSize.ShrinkToAspectRatio(Engine.InternalGameResolution) * scale;

        RenderBox = new Box(
            position: (FullSize - scaledScreenSize) / 2,
            size: scaledScreenSize);
    }
}