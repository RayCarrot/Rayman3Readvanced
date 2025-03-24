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
        Vector2 defaultResolution = new(384, 216);
        const int defaultWindowScale = 4;

        // Display
        DisplayMode = DisplayMode.Fullscreen;
        FullscreenResolution = new Point(defaultDisplayMode.Width, defaultDisplayMode.Height);
        WindowPosition = new Point(0, 0);
        WindowResolution = (defaultResolution * defaultWindowScale).ToPoint();
        WindowIsMaximized = false;
        LockWindowAspectRatio = true;
        
        // Game
        LastPlayedGbaSaveSlot = null;
        LastPlayedNGageSaveSlot = null;
        Language = "en";
        InternalGameResolution = defaultResolution;
        UseReadvancedLogo = true;
        UseModernPauseDialog = true;
        CanSkipTextBoxes = true;
        AddProjectilesWhenNeeded = true;

        // Controls
        Controls = new Dictionary<Input, Keys>();
        
        // Sound
        SfxVolume = 1;
        MusicVolume = 1;
        
        // Debug
        WriteSerializerLog = false;
    }

    #endregion

    #region Private Constant Fields

    private const string DisplaySection = "Display";
    private const string GameSection = "Game";
    private const string ControlsSection = "Controls";
    private const string SoundSection = "Sound";
    private const string DebugSection = "Debug";

    #endregion

    #region Public Properties

    // Display
    public DisplayMode DisplayMode { get; set; }
    public Point FullscreenResolution { get; set; }
    public Point WindowPosition { get; set; }
    public Point WindowResolution { get; set; }
    public bool WindowIsMaximized { get; set; }
    public bool LockWindowAspectRatio { get; set; }

    // Game
    public int? LastPlayedGbaSaveSlot { get; set; }
    public int? LastPlayedNGageSaveSlot { get; set; }
    public string Language { get; set; }
    public Vector2? InternalGameResolution { get; set; } // Null to use original resolution
    public bool UseReadvancedLogo { get; set; }
    public bool UseModernPauseDialog { get; set; }
    public bool CanSkipTextBoxes { get; set; }
    public bool AddProjectilesWhenNeeded { get; set; }

    // Controls
    public Dictionary<Input, Keys> Controls { get; set; }

    // Sound
    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }

    // Debug
    public bool WriteSerializerLog { get; set; }

    #endregion

    #region Public Methods

    public void Serialize(BaseIniSerializer serializer)
    {
        // Display
        DisplayMode = serializer.Serialize<DisplayMode>(DisplayMode, DisplaySection, "DisplayMode");
        FullscreenResolution = serializer.Serialize<Point>(FullscreenResolution, DisplaySection, "FullscreenResolution");
        WindowPosition = serializer.Serialize<Point>(WindowPosition, DisplaySection, "WindowPosition");
        WindowResolution = serializer.Serialize<Point>(WindowResolution, DisplaySection, "WindowResolution");
        WindowIsMaximized = serializer.Serialize<bool>(WindowIsMaximized, DisplaySection, "WindowIsMaximized");
        LockWindowAspectRatio = serializer.Serialize<bool>(LockWindowAspectRatio, DisplaySection, "LockWindowAspectRatio");

        // Game
        LastPlayedGbaSaveSlot = serializer.Serialize<int?>(LastPlayedGbaSaveSlot, GameSection, "LastPlayedGbaSaveSlot");
        LastPlayedNGageSaveSlot = serializer.Serialize<int?>(LastPlayedNGageSaveSlot, GameSection, "LastPlayedNGageSaveSlot");
        Language = serializer.Serialize<string>(Language, GameSection, "Language");
        InternalGameResolution = serializer.Serialize<Vector2?>(InternalGameResolution, GameSection, "InternalGameResolution");
        UseReadvancedLogo = serializer.Serialize<bool>(UseReadvancedLogo, GameSection, "UseReadvancedLogo");
        UseModernPauseDialog = serializer.Serialize<bool>(UseModernPauseDialog, GameSection, "UseModernPauseDialog");
        CanSkipTextBoxes = serializer.Serialize<bool>(CanSkipTextBoxes, GameSection, "CanSkipTextBoxes");
        AddProjectilesWhenNeeded = serializer.Serialize<bool>(AddProjectilesWhenNeeded, GameSection, "AddProjectilesWhenNeeded");

        // Controls
        Controls = serializer.SerializeDictionary<Input, Keys>(Controls, ControlsSection);

        // Sound
        MusicVolume = serializer.Serialize<float>(MusicVolume, SoundSection, "MusicVolume");
        SfxVolume = serializer.Serialize<float>(SfxVolume, SoundSection, "SfxVolume");

        // Debug
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