using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ControlOptionsMenuOption : OptionsMenuOption
{
    public ControlOptionsMenuOption(string text, Input input, bool isDebugOption = false) : base(text, null, isDebugOption)
    {
        Input = input;
    }

    public override bool ShowArrows => false;

    public Input Input { get; }

    private void UpdateSelection()
    {
        Keys key = InputManager.GetKey(Input);
        ValueTextObject.Text = InputManager.GetKeyName(key).ToUpper();
    }

    private void UpdateInput(Keys key, IReadOnlyList<OptionsMenuOption> options)
    {
        Keys prevKey = Engine.Config.Controls.Controls[Input];
        Engine.Config.Controls.Controls[Input] = key;

        // Set as pressed key to avoid it being seen as just having pressed this input
        if (InputManager.TryGetGbaInput(Input, out GbaInput gbaInput))
            JoyPad.Current.KeyStatus |= gbaInput;

        // Check if key is used elsewhere
        foreach (Input input in Enum.GetValues<Input>())
        {
            // Swap
            if (input != Input && Engine.Config.Controls.Controls[input] == key)
            {
                Engine.Config.Controls.Controls[input] = prevKey;

                if (InputManager.TryGetGbaInput(input, out GbaInput gbaInput2))
                    JoyPad.Current.KeyStatus |= gbaInput2;

                ControlOptionsMenuOption option = options.OfType<ControlOptionsMenuOption>().FirstOrDefault(x => x.Input == input);
                option?.UpdateSelection();
            }
        }
    }

    public override void Reset(IReadOnlyList<OptionsMenuOption> options)
    {
        UpdateSelection();
    }

    public override EditStepResult EditStep(IReadOnlyList<OptionsMenuOption> options)
    {
        ValueTextObject.Text = "PRESS A KEY";

        Keys[] pressedKeys = InputManager.GetPressedKeys();

        if (pressedKeys.Length == 0)
            return EditStepResult.None;

        Keys key = pressedKeys[0];

        if (!InputManager.IsButtonJustPressed(key))
            return EditStepResult.None;

        UpdateInput(key, options);
        UpdateSelection();

        return EditStepResult.Apply;
    }
}