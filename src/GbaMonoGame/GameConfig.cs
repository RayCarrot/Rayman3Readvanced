using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GbaMonoGame;

public class GameConfig
{
    #region Constructor

    public GameConfig()
    {
        DisplayMode defaultDisplayMode = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Last();

        FullscreenResolution = new Point(defaultDisplayMode.Width, defaultDisplayMode.Height);
        IsFullscreen = true;
        GbaWindowBounds = new Rectangle(0, 0, 240 * 4, 160 * 4);
        NGageWindowBounds = new Rectangle(0, 0, 176 * 4, 208 * 4);
        InternalGameResolution = null;
        Controls = new Dictionary<Input, Keys>();
        SfxVolume = 1;
        MusicVolume = 1;
        WriteSerializerLog = false;
    }

    #endregion

    #region Events

    public event EventHandler ConfigChanged;

    #endregion

    #region Public Properties

    // Display
    [JsonProperty("fullscreenResolution")] public Point FullscreenResolution { get; set; }
    [JsonProperty("isFullscreen")] public bool IsFullscreen { get; set; }
    [JsonProperty("gbaWindowBounds")] public Rectangle GbaWindowBounds { get; set; }
    [JsonProperty("nGageWindowBounds")] public Rectangle NGageWindowBounds { get; set; }
    
    // Game
    [JsonProperty("internalGameResolution")] public Point? InternalGameResolution { get; set; }

    // Controls
    [JsonProperty("controls")] public Dictionary<Input, Keys> Controls { get; set; }

    // Sound
    [JsonProperty("sfxVolume")] public float SfxVolume { get; set; }
    [JsonProperty("musicVolume")] public float MusicVolume { get; set; }

    // Debug
    [JsonProperty("writeSerializerLog")] public bool WriteSerializerLog { get; set; }

    #endregion

    #region Private Methods

    private static JsonSerializerSettings GetJsonSettings()
    {
        JsonSerializerSettings settings = new()
        {
            Formatting = Formatting.Indented,
        };

        settings.Converters.Add(new StringEnumConverter());

        return settings;
    }

    #endregion

    #region Public Methods

    public void Apply()
    {
        ConfigChanged?.Invoke(this, EventArgs.Empty);
    }

    public static GameConfig Load(string filePath)
    {
        // Read the config
        GameConfig config;
        if (File.Exists(filePath))
            config = JsonConvert.DeserializeObject<GameConfig>(File.ReadAllText(filePath), GetJsonSettings());
        else
            config = new GameConfig();

        // Make sure all inputs are defined
        foreach (Input input in Enum.GetValues<Input>())
        {
            if (!config.Controls.ContainsKey(input))
                config.Controls[input] = InputManager.GetDefaultKey(input);
        }

        return config;
    }

    public void Save(string filePath)
    {
        Apply();
        File.WriteAllText(filePath, JsonConvert.SerializeObject(this, GetJsonSettings()));
    }

    #endregion
}