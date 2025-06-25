using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ControlOptionsMenuOption : OptionsMenuOption
{
    public ControlOptionsMenuOption(string text, Input input) : base(text, null)
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
        Keys prevKey = Engine.Config.Controls[Input];
        Engine.Config.Controls[Input] = key;

        // Set as pressed key to avoid it being seen as just having pressed this input
        JoyPad.Current.KeyStatus |= InputManager.GetGbaInput(Input);

        // Check if key is used elsewhere
        foreach (Input input in Enum.GetValues<Input>())
        {
            // Swap
            if (input != Input && Engine.Config.Controls[input] == key)
            {
                Engine.Config.Controls[input] = prevKey;
                JoyPad.Current.KeyStatus |= InputManager.GetGbaInput(input);

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