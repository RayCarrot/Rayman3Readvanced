using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public record ControlsGameConfig : IniSectionObject
{
    public ControlsGameConfig()
    {
        KeyboardControls = new Dictionary<Input, Keys>();
        GamePadControls = new Dictionary<Input, Buttons>();
        EnabledGamePadVibration = true;
    }

    public override string SectionKey => "Controls";

    public Dictionary<Input, Keys> KeyboardControls { get; set; }
    public Dictionary<Input, Buttons> GamePadControls { get; set; }
    public bool EnabledGamePadVibration { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        KeyboardControls = serializer.SerializeDictionary<Input, Keys>(KeyboardControls, "KeyboardControls");
        GamePadControls = serializer.SerializeDictionary<Input, Buttons>(GamePadControls, "GamePadControls");
        EnabledGamePadVibration = serializer.Serialize<bool>(EnabledGamePadVibration, "EnabledGamePadVibration");

        // Make sure all inputs are defined
        foreach (Input input in Enum.GetValues<Input>())
        {
            if (!KeyboardControls.ContainsKey(input))
                KeyboardControls[input] = InputManager.GetDefaultKey(input);
        }
        foreach (Input input in Enum.GetValues<Input>())
        {
            if (!GamePadControls.ContainsKey(input))
                GamePadControls[input] = InputManager.GetDefaultButton(input);
        }
    }
}