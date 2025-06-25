using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class DisplayGameConfig : IniSectionObject
{
    public DisplayGameConfig()
    {
        Microsoft.Xna.Framework.Graphics.DisplayMode defaultDisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        Vector2 defaultResolution = Resolution.Modern;
        const int defaultWindowScale = 4;

        Language = "en";
        DisplayMode = DisplayMode.Fullscreen;
        FullscreenResolution = new Point(defaultDisplayMode.Width, defaultDisplayMode.Height);
        WindowPosition = new Point(0, 0);
        WindowResolution = (defaultResolution * defaultWindowScale).ToPoint();
        WindowIsMaximized = false;
        LockWindowAspectRatio = true;
        DisableCameraShake = false;
    }

    public override string SectionKey => "Display";

    public string Language { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public Point FullscreenResolution { get; set; }
    public Point WindowPosition { get; set; }
    public Point WindowResolution { get; set; }
    public bool WindowIsMaximized { get; set; }
    public bool LockWindowAspectRatio { get; set; }
    public bool DisableCameraShake { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        Language = serializer.Serialize<string>(Language, "Language");
        DisplayMode = serializer.Serialize<DisplayMode>(DisplayMode, "DisplayMode");
        FullscreenResolution = serializer.Serialize<Point>(FullscreenResolution, "FullscreenResolution");
        WindowPosition = serializer.Serialize<Point>(WindowPosition, "WindowPosition");
        WindowResolution = serializer.Serialize<Point>(WindowResolution, "WindowResolution");
        WindowIsMaximized = serializer.Serialize<bool>(WindowIsMaximized, "WindowIsMaximized");
        LockWindowAspectRatio = serializer.Serialize<bool>(LockWindowAspectRatio, "LockWindowAspectRatio");
        DisableCameraShake = serializer.Serialize<bool>(DisableCameraShake, "DisableCameraShake");
    }
}