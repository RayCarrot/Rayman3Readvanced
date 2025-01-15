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
        Point defaultResolution = new(384, 216);
        const int defaultWindowScale = 4;

        // Display
        WindowPosition = new Point(0, 0);
        WindowResolution = defaultResolution * new Point(defaultWindowScale);
        WindowIsMaximized = false;
        LockWindowAspectRatio = true;
        FullscreenResolution = new Point(defaultDisplayMode.Width, defaultDisplayMode.Height);
        DisplayMode = DisplayMode.Fullscreen;
        
        // Game
        InternalGameResolution = defaultResolution;
        Language = "en";
        LastPlayedGbaSaveSlot = null;
        
        Controls = new Dictionary<Input, Keys>();
        SfxVolume = 1;
        MusicVolume = 1;
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
    public Point WindowPosition { get; set; }
    public Point WindowResolution { get; set; }
    public bool WindowIsMaximized { get; set; }
    public bool LockWindowAspectRatio { get; set; }
    public Point FullscreenResolution { get; set; }
    public DisplayMode DisplayMode { get; set; }

    // Game
    public Point InternalGameResolution { get; set; }
    public string Language { get; set; }
    public int? LastPlayedGbaSaveSlot { get; set; }
    public int? LastPlayedNGageSaveSlot { get; set; }

    // Controls
    public Dictionary<Input, Keys> Controls { get; set; }

    // Sound
    public float SfxVolume { get; set; }
    public float MusicVolume { get; set; }

    // Debug
    public bool WriteSerializerLog { get; set; }

    #endregion

    #region Public Methods

    public void Serialize(BaseIniSerializer serializer)
    {
        // Display
        WindowPosition = serializer.Serialize<Point>(WindowPosition, DisplaySection, "WindowPosition");
        WindowResolution = serializer.Serialize<Point>(WindowResolution, DisplaySection, "WindowResolution");
        WindowIsMaximized = serializer.Serialize<bool>(WindowIsMaximized, DisplaySection, "WindowIsMaximized");
        LockWindowAspectRatio = serializer.Serialize<bool>(LockWindowAspectRatio, DisplaySection, "LockWindowAspectRatio");
        FullscreenResolution = serializer.Serialize<Point>(FullscreenResolution, DisplaySection, "FullscreenResolution");
        DisplayMode = serializer.Serialize<DisplayMode>(DisplayMode, DisplaySection, "DisplayMode");

        // Game
        InternalGameResolution = serializer.Serialize<Point>(InternalGameResolution, GameSection, "InternalGameResolution");
        Language = serializer.Serialize<string>(Language, GameSection, "Language");
        LastPlayedGbaSaveSlot = serializer.Serialize<int?>(LastPlayedGbaSaveSlot, GameSection, "LastPlayedGbaSaveSlot");
        LastPlayedNGageSaveSlot = serializer.Serialize<int?>(LastPlayedNGageSaveSlot, GameSection, "LastPlayedNGageSaveSlot");

        // Controls
        Controls = serializer.SerializeDictionary<Input, Keys>(Controls, ControlsSection);

        // Sound
        SfxVolume = serializer.Serialize<float>(SfxVolume, SoundSection, "SfxVolume");
        MusicVolume = serializer.Serialize<float>(MusicVolume, SoundSection, "MusicVolume");

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