using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public record DisplayGameConfig : IniSectionObject
{
    public DisplayGameConfig()
    {
        Microsoft.Xna.Framework.Graphics.DisplayMode defaultDisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        Vector2 defaultResolution = Resolution.Modern;
        const int defaultWindowScale = 4;

        Language = "en";
        DisplayMode = DisplayMode.Windowed;
        AltEnterToggle = DisplayMode.Borderless;
        FullscreenResolution = new Point(defaultDisplayMode.Width, defaultDisplayMode.Height);
        WindowPosition = new Point(0, 0);
        WindowResolution = (defaultResolution * defaultWindowScale).ToPoint();
        WindowIsMaximized = false;
        LockWindowAspectRatio = true;
        DisableCameraShake = false;
        DisableFlashingLights = false;
    }

    public override string SectionKey => "Display";

    public string Language { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public DisplayMode? AltEnterToggle { get; set; }
    public Point FullscreenResolution { get; set; }
    public Point WindowPosition { get; set; }
    public Point WindowResolution { get; set; }
    public bool WindowIsMaximized { get; set; }
    public bool LockWindowAspectRatio { get; set; }
    public bool DisableCameraShake { get; set; }
    public bool DisableFlashingLights { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        Language = serializer.Serialize<string>(Language, "Language");
        DisplayMode = serializer.Serialize<DisplayMode>(DisplayMode, "DisplayMode");
        AltEnterToggle = serializer.Serialize<DisplayMode?>(AltEnterToggle, "AltEnterToggle");
        FullscreenResolution = serializer.Serialize<Point>(FullscreenResolution, "FullscreenResolution");
        WindowPosition = serializer.Serialize<Point>(WindowPosition, "WindowPosition");
        WindowResolution = serializer.Serialize<Point>(WindowResolution, "WindowResolution");
        WindowIsMaximized = serializer.Serialize<bool>(WindowIsMaximized, "WindowIsMaximized");
        LockWindowAspectRatio = serializer.Serialize<bool>(LockWindowAspectRatio, "LockWindowAspectRatio");
        DisableCameraShake = serializer.Serialize<bool>(DisableCameraShake, "DisableCameraShake");
        DisableFlashingLights = serializer.Serialize<bool>(DisableFlashingLights, "DisableFlashingLights");
    }
}