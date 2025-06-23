using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public class GameConfig
{
    #region Constructor

    public GameConfig()
    {
        Microsoft.Xna.Framework.Graphics.DisplayMode defaultDisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        Vector2 defaultResolution = Resolution.Modern;
        const int defaultWindowScale = 4;

        // General
        LastPlayedGbaSaveSlot = null;
        LastPlayedNGageSaveSlot = null;

        // Display
        Language = "en";
        DisplayMode = DisplayMode.Fullscreen;
        FullscreenResolution = new Point(defaultDisplayMode.Width, defaultDisplayMode.Height);
        WindowPosition = new Point(0, 0);
        WindowResolution = (defaultResolution * defaultWindowScale).ToPoint();
        WindowIsMaximized = false;
        LockWindowAspectRatio = true;
        DisableCameraShake = false;
        
        // Controls
        Controls = new Dictionary<Input, Keys>();
        
        // Sound
        SfxVolume = 1;
        MusicVolume = 1;

        // Tweaks
        InternalGameResolution = defaultResolution;
        UseReadvancedLogo = true;
        UseModernPauseDialog = true;
        CanSkipTextBoxes = true;
        AddProjectilesWhenNeeded = true;
        FixBugs = true;
        UseGbaEffectsOnNGage = true;
        UseExtendedBackgrounds = true;

        // Debug
        DebugModeEnabled = false;
        WriteSerializerLog = false;
    }

    #endregion

    #region Private Constant Fields

    private const string GeneralSection = "General";
    private const string DisplaySection = "Display";
    private const string TweaksSection = "Tweaks";
    private const string ControlsSection = "Controls";
    private const string SoundSection = "Sound";
    private const string DebugSection = "Debug";

    #endregion

    #region Public Properties

    // General
    public int? LastPlayedGbaSaveSlot { get; set; }
    public int? LastPlayedNGageSaveSlot { get; set; }

    // Display
    public string Language { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public Point FullscreenResolution { get; set; }
    public Point WindowPosition { get; set; }
    public Point WindowResolution { get; set; }
    public bool WindowIsMaximized { get; set; }
    public bool LockWindowAspectRatio { get; set; }
    public bool DisableCameraShake { get; set; }

    // Controls
    public Dictionary<Input, Keys> Controls { get; set; }

    // Sound
    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }

    // Tweaks
    public Vector2? InternalGameResolution { get; set; } // Null to use original resolution
    public bool UseReadvancedLogo { get; set; }
    public bool UseModernPauseDialog { get; set; }
    public bool CanSkipTextBoxes { get; set; }
    public bool AddProjectilesWhenNeeded { get; set; }
    public bool FixBugs { get; set; }
    public bool UseGbaEffectsOnNGage { get; set; }
    public bool UseExtendedBackgrounds { get; set; }

    // Debug (can only be manually modified)
    public bool DebugModeEnabled { get; set; }
    public bool WriteSerializerLog { get; set; }

    #endregion

    #region Public Methods

    public void Serialize(BaseIniSerializer serializer)
    {
        // General
        LastPlayedGbaSaveSlot = serializer.Serialize<int?>(LastPlayedGbaSaveSlot, GeneralSection, "LastPlayedGbaSaveSlot");
        LastPlayedNGageSaveSlot = serializer.Serialize<int?>(LastPlayedNGageSaveSlot, GeneralSection, "LastPlayedNGageSaveSlot");

        // Display
        Language = serializer.Serialize<string>(Language, DisplaySection, "Language");
        DisplayMode = serializer.Serialize<DisplayMode>(DisplayMode, DisplaySection, "DisplayMode");
        FullscreenResolution = serializer.Serialize<Point>(FullscreenResolution, DisplaySection, "FullscreenResolution");
        WindowPosition = serializer.Serialize<Point>(WindowPosition, DisplaySection, "WindowPosition");
        WindowResolution = serializer.Serialize<Point>(WindowResolution, DisplaySection, "WindowResolution");
        WindowIsMaximized = serializer.Serialize<bool>(WindowIsMaximized, DisplaySection, "WindowIsMaximized");
        LockWindowAspectRatio = serializer.Serialize<bool>(LockWindowAspectRatio, DisplaySection, "LockWindowAspectRatio");
        DisableCameraShake = serializer.Serialize<bool>(DisableCameraShake, DisplaySection, "DisableCameraShake");

        // Controls
        Controls = serializer.SerializeDictionary<Input, Keys>(Controls, ControlsSection);

        // Sound
        MusicVolume = serializer.Serialize<float>(MusicVolume, SoundSection, "MusicVolume");
        SfxVolume = serializer.Serialize<float>(SfxVolume, SoundSection, "SfxVolume");

        // Tweaks
        InternalGameResolution = serializer.Serialize<Vector2?>(InternalGameResolution, TweaksSection, "InternalGameResolution");
        UseReadvancedLogo = serializer.Serialize<bool>(UseReadvancedLogo, TweaksSection, "UseReadvancedLogo");
        UseModernPauseDialog = serializer.Serialize<bool>(UseModernPauseDialog, TweaksSection, "UseModernPauseDialog");
        CanSkipTextBoxes = serializer.Serialize<bool>(CanSkipTextBoxes, TweaksSection, "CanSkipTextBoxes");
        AddProjectilesWhenNeeded = serializer.Serialize<bool>(AddProjectilesWhenNeeded, TweaksSection, "AddProjectilesWhenNeeded");
        FixBugs = serializer.Serialize<bool>(FixBugs, TweaksSection, "FixBugs");
        UseGbaEffectsOnNGage = serializer.Serialize<bool>(UseGbaEffectsOnNGage, TweaksSection, "UseGbaEffectsOnNGage");
        UseExtendedBackgrounds = serializer.Serialize<bool>(UseExtendedBackgrounds, TweaksSection, "UseExtendedBackgrounds");

        // Debug
        DebugModeEnabled = serializer.Serialize<bool>(DebugModeEnabled, DebugSection, "DebugModeEnabled");
        WriteSerializerLog = serializer.Serialize<bool>(WriteSerializerLog, DebugSection, "WriteSerializerLog");

        // Make sure all inputs are defined
        foreach (Input input in Enum.GetValues<Input>())
        {
            if (!Controls.ContainsKey(input))
                Controls[input] = InputManager.GetDefaultKey(input);
        }
    }

    #endregion
}