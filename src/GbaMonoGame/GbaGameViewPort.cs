namespace GbaMonoGame;

public class GbaGameViewPort
{
    public Vector2 ScreenSize { get; private set; }

    public void Resize(Vector2 newScreenSize)
    {
        ScreenSize = newScreenSize;
    }
}