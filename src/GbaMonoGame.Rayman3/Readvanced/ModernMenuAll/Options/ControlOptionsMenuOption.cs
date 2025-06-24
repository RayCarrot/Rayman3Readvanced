using System;
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

    private bool UpdateInput(Keys key)
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
                return true;
            }
        }

        return false;
    }

    public override void Reset()
    {
        UpdateSelection();
    }

    public override EditStepResult EditStep()
    {
        ValueTextObject.Text = "PRESS A KEY";

        Keys[] pressedKeys = InputManager.GetPressedKeys();

        if (pressedKeys.Length == 0)
            return EditStepResult.None;

        Keys key = pressedKeys[0];

        if (!InputManager.IsButtonJustPressed(key))
            return EditStepResult.None;

        bool resetAll = UpdateInput(key);
        UpdateSelection();

        return resetAll ? EditStepResult.ConfirmResetAll : EditStepResult.Confirm;
    }
}