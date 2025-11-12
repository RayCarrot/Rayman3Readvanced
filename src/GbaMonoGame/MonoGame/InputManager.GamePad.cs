using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public static partial class InputManager
{
    private static readonly Buttons[] _allButtons = Enum.GetValues<Buttons>();
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

    // TODO: Allow custom mapping
    // TODO: Allow both sticks and d-pad for movement (stick is always mapped to buttons, with d-pad being optional?)
    // TODO: Map debug buttons (are there enough buttons?)
    // TODO: Ideally GBA B button would be mapped to both B and X since it acts both as attack and going back
    public static Buttons GetButton(Input input) => input switch
    {
        Input.Gba_A => Buttons.A,
        Input.Gba_B => Buttons.B,
        Input.Gba_Select => Buttons.Back,
        Input.Gba_Start => Buttons.Start,
        //Input.Gba_Right => Buttons.LeftThumbstickRight,
        //Input.Gba_Left => Buttons.LeftThumbstickLeft,
        //Input.Gba_Up => Buttons.LeftThumbstickUp,
        //Input.Gba_Down => Buttons.LeftThumbstickDown,
        Input.Gba_Right => Buttons.DPadRight,
        Input.Gba_Left => Buttons.DPadLeft,
        Input.Gba_Up => Buttons.DPadUp,
        Input.Gba_Down => Buttons.DPadDown,
        Input.Gba_R => Buttons.RightShoulder,
        Input.Gba_L => Buttons.LeftShoulder,
        Input.Debug_ToggleDebugMode => Buttons.None,
        Input.Debug_TogglePause => Buttons.LeftStick,
        Input.Debug_StepOneFrame => Buttons.LeftTrigger,
        Input.Debug_SpeedUp => Buttons.RightTrigger,
        Input.Debug_ToggleDisplayBoxes => Buttons.None,
        Input.Debug_ToggleDisplayCollision => Buttons.None,
        Input.Debug_ToggleNoClip => Buttons.None,
        _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
    };

    public static bool IsButtonPressed(Buttons button) => _gamePadState.IsButtonDown(button);
    public static bool IsButtonReleased(Buttons button) => _gamePadState.IsButtonUp(button);
    public static bool IsButtonJustPressed(Buttons button) => _gamePadState.IsButtonDown(button) && _previousGamePadState.IsButtonUp(button);
    public static bool IsButtonJustReleased(Buttons button) => _gamePadState.IsButtonUp(button) && _previousGamePadState.IsButtonDown(button);

    public static bool IsAnyButtonPressed()
    {
        foreach (Buttons button in _allButtons)
        {
            if (IsButtonPressed(button))
                return true;
        }

        return false;
    }
}