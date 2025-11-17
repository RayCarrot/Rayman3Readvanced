using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public static partial class InputManager
{
    private const float StickThreshhold = 0.5f;

    private static readonly Buttons[] _validButtons = 
    [
        Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight,
        Buttons.Start, Buttons.Back, Buttons.BigButton,
        Buttons.LeftStick, Buttons.RightStick,
        Buttons.LeftShoulder, Buttons.RightShoulder,
        Buttons.LeftTrigger, Buttons.RightTrigger,
        Buttons.A, Buttons.B, Buttons.X, Buttons.Y,
    ];
    private static GamePadState _previousGamePadState;
    private static GamePadState _gamePadState;

    public static bool IsGamePadConnected => _gamePadState.IsConnected;

    private static void UpdateGamePad()
    {
        // TODO: Check capabilities?
        // GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);

        _previousGamePadState = _gamePadState;
        _gamePadState = GamePad.GetState(PlayerIndex.One);
    }

    private static Input GetInputsFromGamePad()
    {
        Input inputs = default;

        foreach (Input input in _allInputs)
        {
            if (IsButtonPressed(GetButton(input)))
                inputs |= input;
        }

        // Always map the left thumbstick to the directional inputs
        if (_gamePadState.ThumbSticks.Left.X > StickThreshhold)
            inputs |= Input.Gba_Right;
        if (_gamePadState.ThumbSticks.Left.X < -StickThreshhold)
            inputs |= Input.Gba_Left;
        if (_gamePadState.ThumbSticks.Left.Y > StickThreshhold)
            inputs |= Input.Gba_Up;
        if (_gamePadState.ThumbSticks.Left.Y < -StickThreshhold)
            inputs |= Input.Gba_Down;

        return inputs;
    }

    public static Buttons GetButton(Input input) => Engine.LocalConfig.Controls.GamePadControls[input];

    // TODO: Map debug buttons using debug modifier key
    public static Buttons GetDefaultButton(Input input)
    {
        return input switch
        {
            // GBA
            Input.Gba_A => Buttons.A,
            Input.Gba_B => Buttons.B,
            Input.Gba_Select => Buttons.Back,
            Input.Gba_Start => Buttons.Start,
            Input.Gba_Right => Buttons.DPadRight,
            Input.Gba_Left => Buttons.DPadLeft,
            Input.Gba_Up => Buttons.DPadUp,
            Input.Gba_Down => Buttons.DPadDown,
            Input.Gba_R => Buttons.RightShoulder,
            Input.Gba_L => Buttons.LeftShoulder,

            // Debug
            Input.Debug_ToggleDebugMode => Buttons.None,
            Input.Debug_TogglePause => Buttons.None,
            Input.Debug_StepOneFrame => Buttons.None,
            Input.Debug_SpeedUp => Buttons.None,
            Input.Debug_ToggleDisplayBoxes => Buttons.None,
            Input.Debug_ToggleDisplayCollision => Buttons.None,
            Input.Debug_ToggleNoClip => Buttons.None,

            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }

    public static string GetButtonName(Buttons button)
    {
        // TODO: Better names to use?
        return button switch
        {
            Buttons.None => "None",
            Buttons.DPadUp => "D-pad up",
            Buttons.DPadDown => "D-pad down",
            Buttons.DPadLeft => "D-pad left",
            Buttons.DPadRight => "D-pad right",
            Buttons.Start => "Start",
            Buttons.Back => "Back",
            Buttons.LeftStick => "Left stick",
            Buttons.RightStick => "Right stick",
            Buttons.LeftShoulder => "Left shoulder",
            Buttons.RightShoulder => "Right shoulder",
            Buttons.BigButton => "Big button",
            Buttons.A => "A",
            Buttons.B => "B",
            Buttons.X => "X",
            Buttons.Y => "Y",
            Buttons.LeftThumbstickLeft => "Left stick left",
            Buttons.RightTrigger => "Right trigger",
            Buttons.LeftTrigger => "Left trigger",
            Buttons.RightThumbstickUp => "Right stick up",
            Buttons.RightThumbstickDown => "Right stick down",
            Buttons.RightThumbstickRight => "Right stick right",
            Buttons.RightThumbstickLeft => "Right stick left",
            Buttons.LeftThumbstickUp => "Left stick up",
            Buttons.LeftThumbstickDown => "Left stick down",
            Buttons.LeftThumbstickRight => "Left stick right",
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }

    public static Buttons GetPressedButton()
    {
        foreach (Buttons button in _validButtons)
        {
            if (IsButtonPressed(button))
                return button;
        }

        return Buttons.None;
    }

    public static bool IsButtonPressed(Buttons button) => _gamePadState.IsButtonDown(button);
    public static bool IsButtonReleased(Buttons button) => _gamePadState.IsButtonUp(button);
    public static bool IsButtonJustPressed(Buttons button) => _gamePadState.IsButtonDown(button) && _previousGamePadState.IsButtonUp(button);
    public static bool IsButtonJustReleased(Buttons button) => _gamePadState.IsButtonUp(button) && _previousGamePadState.IsButtonDown(button);

    public static bool IsAnyButtonPressed()
    {
        foreach (Buttons button in _validButtons)
        {
            if (IsButtonPressed(button))
                return true;
        }

        if (Math.Abs(_gamePadState.ThumbSticks.Left.X) > StickThreshhold ||
            Math.Abs(_gamePadState.ThumbSticks.Left.Y) > StickThreshhold)
            return true;

        return false;
    }
}