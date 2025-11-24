using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public record ControlsGameConfig : IniSectionObject
{
    public ControlsGameConfig()
    {
        EnabledGamePadVibration = true;
        UseModernButtonMapping = true;
        KeyboardControls = new Dictionary<Input, Keys>();
        GamePadControls = new Dictionary<Input, Buttons>();
    }

    public override string SectionKey => "Controls";

    public bool EnabledGamePadVibration { get; set; }
    public bool UseModernButtonMapping { get; set; }
    public Dictionary<Input, Keys> KeyboardControls { get; set; }
    public Dictionary<Input, Buttons> GamePadControls { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        EnabledGamePadVibration = serializer.Serialize<bool>(EnabledGamePadVibration, "EnabledGamePadVibration");
        UseModernButtonMapping = serializer.Serialize<bool>(UseModernButtonMapping, "UseModernButtonMapping");
        KeyboardControls = serializer.SerializeDictionary<Input, Keys>(KeyboardControls, "KeyboardControls");
        GamePadControls = serializer.SerializeDictionary<Input, Buttons>(GamePadControls, "GamePadControls");

        // Make sure all inputs are defined
        foreach (Input input in Enum.GetValues<Input>())
        {
            if (!KeyboardControls.ContainsKey(input))
                KeyboardControls[input] = InputManager.GetDefaultKey(input);

            if (!GamePadControls.ContainsKey(input))
                GamePadControls[input] = InputManager.GetDefaultButton(input);
        }
    }
}