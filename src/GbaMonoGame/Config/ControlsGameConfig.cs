using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public record ControlsGameConfig : IniSectionObject
{
    public ControlsGameConfig()
    {
        Controls = new Dictionary<Input, Keys>();
    }

    public override string SectionKey => "Controls";

    public Dictionary<Input, Keys> Controls { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        Controls = serializer.SerializeDictionary<Input, Keys>(Controls);

        // Make sure all inputs are defined
        foreach (Input input in Enum.GetValues<Input>())
        {
            if (!Controls.ContainsKey(input))
                Controls[input] = InputManager.GetDefaultKey(input);
        }
    }
}