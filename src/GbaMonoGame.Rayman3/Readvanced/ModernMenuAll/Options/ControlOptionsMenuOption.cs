using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ControlOptionsMenuOption : OptionsMenuOption
{
    public ControlOptionsMenuOption(string text, Input input, bool isDebugOption = false, bool isAvailableOnGamePad = true) 
        : base(text, null, isDebugOption)
    {
        Input = input;
        IsAvailableOnGamePad = isAvailableOnGamePad;
    }

    public override bool CanEdit => !(InputManager.InputMode == InputMode.GamePad && !IsAvailableOnGamePad);

    public override bool ShowArrows => false;

    public Input Input { get; }
    public bool IsAvailableOnGamePad { get; }
    public InputMode? PreviousInputMode { get; set; }

    private void UpdateSelection()
    {
        if (InputManager.InputMode == InputMode.Keyboard)
        {
            Keys key = InputManager.GetKey(Input);
            ValueTextObject.Text = InputManager.GetKeyName(key).ToUpper();
        }
        else if (InputManager.InputMode == InputMode.GamePad)
        {
            if (IsAvailableOnGamePad)
            {
                Buttons button = InputManager.GetButton(Input);
                ValueTextObject.Text = InputManager.GetButtonName(button).ToUpper();
            }
            else
            {
                ValueTextObject.Text = "N/A";
            }
        }
        else
        {
            throw new InvalidOperationException($"Input mode {InputManager.InputMode} is not supported");
        }

        PreviousInputMode = InputManager.InputMode;
    }

    private void UpdateKeyboardInput(Keys key, IReadOnlyList<OptionsMenuOption> options)
    {
        Keys prevKey = Engine.LocalConfig.Controls.KeyboardControls[Input];
        Engine.LocalConfig.Controls.KeyboardControls[Input] = key;

        // Set as pressed key to avoid it being seen as just having pressed this input
        if (InputManager.TryGetGbaInput(Input, out GbaInput gbaInput))
            JoyPad.Current.KeyStatus |= gbaInput;

        // Check if key is used elsewhere
        foreach (Input input in Enum.GetValues<Input>())
        {
            // Swap
            if (input != Input &&
                Engine.LocalConfig.Controls.KeyboardControls[input] == key)
            {
                Engine.LocalConfig.Controls.KeyboardControls[input] = prevKey;

                if (InputManager.TryGetGbaInput(input, out GbaInput gbaInput2))
                    JoyPad.Current.KeyStatus |= gbaInput2;

                ControlOptionsMenuOption option = options.OfType<ControlOptionsMenuOption>().FirstOrDefault(x => x.Input == input);
                option?.UpdateSelection();
            }
        }
    }

    private void UpdateGamePadInput(Buttons button, IReadOnlyList<OptionsMenuOption> options)
    {
        Buttons prevButton = Engine.LocalConfig.Controls.GamePadControls[Input];
        Engine.LocalConfig.Controls.GamePadControls[Input] = button;

        // Set as pressed key to avoid it being seen as just having pressed this input
        if (InputManager.TryGetGbaInput(Input, out GbaInput gbaInput))
            JoyPad.Current.KeyStatus |= gbaInput;

        // Check if key is used elsewhere
        foreach (Input input in Enum.GetValues<Input>())
        {
            // Swap
            if (input != Input && 
                Engine.LocalConfig.Controls.GamePadControls[input] == button)
            {
                Engine.LocalConfig.Controls.GamePadControls[input] = prevButton;

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
        if (InputManager.InputMode == InputMode.Keyboard)
        {
            ValueTextObject.Text = "PRESS A KEY";

            Keys pressedKey = InputManager.GetPressedKey();

            // Don't allow mapping escape since we hard-code that
            if (pressedKey is Keys.None or Keys.Escape)
                return EditStepResult.None;

            if (!InputManager.IsKeyJustPressed(pressedKey))
                return EditStepResult.None;

            UpdateKeyboardInput(pressedKey, options);
            UpdateSelection();

            return EditStepResult.Apply;
        }
        else if (InputManager.InputMode == InputMode.GamePad)
        {
            ValueTextObject.Text = "PRESS A BUTTON";

            Buttons pressedButton = InputManager.GetPressedButton();

            if (pressedButton == Buttons.None)
                return EditStepResult.None;

            if (!InputManager.IsButtonJustPressed(pressedButton))
                return EditStepResult.None;

            UpdateGamePadInput(pressedButton, options);
            UpdateSelection();

            return EditStepResult.Apply;
        }
        else
        {
            throw new InvalidOperationException($"Input mode {InputManager.InputMode} is not supported");
        }
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        // Update text if the input mode changes
        if (InputManager.InputMode != PreviousInputMode)
            UpdateSelection();

        base.Draw(animationPlayer);
    }
}