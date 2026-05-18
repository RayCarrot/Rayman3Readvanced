using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.Tests;

public class MockGameWindowManager : IGameWindowManager
{
    public Point WindowResolution { get; set; }
    public Point FullscreenResolution { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public bool VSync { get; set; }

    public Point GetResolution() => DisplayMode == DisplayMode.Windowed ? WindowResolution : FullscreenResolution;
    public void SetTitle(string title) { }
    public bool IsResizable() => false;
    public void SetResizeMode(bool allowResize, Point minSize, Point maxSize) { }
    public void SaveState() { }
    public void ApplyState() { }
}