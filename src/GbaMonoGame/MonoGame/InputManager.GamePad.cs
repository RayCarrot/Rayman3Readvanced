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

    private static float _vibrationStrength;
    private static int _vibrationTimer;

    public static bool IsGamePadConnected => _gamePadState.IsConnected;

    private static void UpdateGamePad()
    {
        _previousGamePadState = _gamePadState;
        _gamePadState = GamePad.GetState(PlayerIndex.One);

        if (_vibrationTimer > 0 && 
            Engine.LocalConfig.Controls.EnabledGamePadVibration && 
            InputMode == InputMode.GamePad && 
            IsGamePadConnected)
        {
            _vibrationTimer--;
            GamePad.SetVibration(PlayerIndex.One, _vibrationStrength, _vibrationStrength);
        }
        else
        {
            _vibrationTimer = 0;
            GamePad.SetVibration(PlayerIndex.One, 0, 0);
        }
    }

    private static Input GetInputsFromGamePad()
    {
        Input inputs = default;

        bool modifierPressed = IsButtonPressed(GetButton(Input.Debug_Modifier));

        foreach (Input input in _allInputs)
        {
            if (RequiresModifier(input) == modifierPressed && IsButtonPressed(GetButton(input)))
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

    public static Buttons GetDefaultButton(Input input)
    {
        return input switch
        {
            // GBA
            Input.Gba_A => Buttons.A,
            Input.Gba_B => Buttons.X,
            Input.Gba_Select => Buttons.B,
            Input.Gba_Start => Buttons.Start,
            Input.Gba_Right => Buttons.DPadRight,
            Input.Gba_Left => Buttons.DPadLeft,
            Input.Gba_Up => Buttons.DPadUp,
            Input.Gba_Down => Buttons.DPadDown,
            Input.Gba_R => Buttons.RightShoulder,
            Input.Gba_L => Buttons.LeftShoulder,

            // Debug
            Input.Debug_Modifier => Buttons.LeftTrigger,
            Input.Debug_ToggleDebugMode => Buttons.LeftShoulder,
            Input.Debug_TogglePause => Buttons.Start,
            Input.Debug_StepOneFrame => Buttons.RightTrigger,
            Input.Debug_SpeedUp => Buttons.RightShoulder,
            Input.Debug_ToggleDisplayBoxes => Buttons.X,
            Input.Debug_ToggleDisplayCollision => Buttons.B,
            Input.Debug_ToggleNoClip => Buttons.A,

            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }

    public static string GetButtonName(Buttons button)
    {
        // TODO: Better names to use? Names based on controller name so we can support PlayStation names?
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

    public static void SetVibration(float strength, int time)
    {
        _vibrationStrength = strength;
        _vibrationTimer = time;
    }

    // TODO: Implement vibration support throughout the game. Maybe tweak values too.
    public static void SetVibration(VibrationStrength strength, VibrationTime time)
    {
        _vibrationStrength = strength switch
        {
            VibrationStrength.VeryWeak => 1 / 100f,
            VibrationStrength.Weak => 1 / 10f,
            VibrationStrength.Medium => 1 / 5f,
            VibrationStrength.Strong => 1 / 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(strength), strength, null)
        };
        _vibrationTimer = time switch
        {
            VibrationTime.Step => 1,
            VibrationTime.Short => 5,
            VibrationTime.Medium => 12,
            VibrationTime.Long => 20,
            _ => throw new ArgumentOutOfRangeException(nameof(time), time, null)
        };
    }
}