using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public interface IGameWindowManager
{
    Point WindowResolution { get; set; }
    Point FullscreenResolution { get; set; }
    DisplayMode DisplayMode { get; set; }
    bool VSync { get; set; }

    Point GetResolution();
    void SetTitle(string title);
    bool IsResizable();
    void SetResizeMode(bool allowResize, Point minSize, Point maxSize);
    void SaveState();
    void ApplyState();
}