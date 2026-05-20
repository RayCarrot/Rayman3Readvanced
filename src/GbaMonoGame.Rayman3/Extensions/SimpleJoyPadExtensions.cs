using System.Collections.Frozen;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Rayman3;

public static class SimpleJoyPadExtensions
{
    private static FrozenDictionary<Rayman3Input, GbaInput[]> Rayman3ModernInputs { get; } = new Dictionary<Rayman3Input, GbaInput[]>
    {
        [Rayman3Input.MenuUp] = [GbaInput.Up],
        [Rayman3Input.MenuDown] = [GbaInput.Down],
        [Rayman3Input.MenuLeft] = [GbaInput.Left],
        [Rayman3Input.MenuRight] = [GbaInput.Right],
        [Rayman3Input.MenuLeftExt] = [GbaInput.Left, GbaInput.L],
        [Rayman3Input.MenuRightExt] = [GbaInput.Right, GbaInput.R],
        [Rayman3Input.MenuConfirm] = [GbaInput.A, GbaInput.Start],
        [Rayman3Input.MenuBack] = [GbaInput.B, GbaInput.Select],
        [Rayman3Input.MenuLostMultiplayerConnectionBack] = [GbaInput.A, GbaInput.B, GbaInput.Start, GbaInput.Select],
        [Rayman3Input.MenuMultiplayerStart] = [GbaInput.A, GbaInput.Start],
        [Rayman3Input.PauseMenuBack] = [GbaInput.B, GbaInput.Select],
        [Rayman3Input.GameCubeMenuConfirm] = [GbaInput.A, GbaInput.Start],
        [Rayman3Input.GameCubeMenuBack] = [GbaInput.B, GbaInput.Select],
        [Rayman3Input.LevelSelectMenuConfirm] = [GbaInput.A, GbaInput.Start],
        [Rayman3Input.LevelSelectMenuBack] = [GbaInput.B, GbaInput.Select],
        [Rayman3Input.StorySkip] = [GbaInput.Start],
        [Rayman3Input.StoryNext] = [GbaInput.A],
        [Rayman3Input.TextBoxSkip] = [GbaInput.Start],
        [Rayman3Input.TextBoxNext] = [GbaInput.A],
        [Rayman3Input.CreditsSkip] = [GbaInput.A, GbaInput.B, GbaInput.Start, GbaInput.Select],
        [Rayman3Input.Pause] = [GbaInput.Start],
        [Rayman3Input.GameOverContinue] = [GbaInput.A, GbaInput.B, GbaInput.Start, GbaInput.Select],
        [Rayman3Input.IntroSkip] = [GbaInput.A, GbaInput.B, GbaInput.Start, GbaInput.Select],
        [Rayman3Input.ActorUp] = [GbaInput.Up],
        [Rayman3Input.ActorDown] = [GbaInput.Down],
        [Rayman3Input.ActorLeft] = [GbaInput.Left],
        [Rayman3Input.ActorRight] = [GbaInput.Right],
        [Rayman3Input.ActorJump] = [GbaInput.A],
        [Rayman3Input.ActorAttack] = [GbaInput.B],
        [Rayman3Input.ActorSpecialLeft] = [GbaInput.L],
        [Rayman3Input.ActorSpecialRight] = [GbaInput.R],
    }.ToFrozenDictionary();
    private static FrozenDictionary<Rayman3Input, GbaInput[]> Rayman3GbaInputs { get; } = new Dictionary<Rayman3Input, GbaInput[]>
    {
        [Rayman3Input.MenuUp] = [GbaInput.Up],
        [Rayman3Input.MenuDown] = [GbaInput.Down],
        [Rayman3Input.MenuLeft] = [GbaInput.Left],
        [Rayman3Input.MenuRight] = [GbaInput.Right],
        [Rayman3Input.MenuLeftExt] = [GbaInput.Left, GbaInput.L],
        [Rayman3Input.MenuRightExt] = [GbaInput.Right, GbaInput.R],
        [Rayman3Input.MenuConfirm] = [GbaInput.A],
        [Rayman3Input.MenuBack] = [GbaInput.B],
        [Rayman3Input.MenuLostMultiplayerConnectionBack] = [GbaInput.Start],
        [Rayman3Input.MenuMultiplayerStart] = [GbaInput.Start],
        [Rayman3Input.PauseMenuBack] = [GbaInput.B, GbaInput.Start],
        [Rayman3Input.GameCubeMenuConfirm] = [GbaInput.A, GbaInput.Start],
        [Rayman3Input.GameCubeMenuBack] = [GbaInput.B],
        [Rayman3Input.LevelSelectMenuConfirm] = [GbaInput.A],
        [Rayman3Input.LevelSelectMenuBack] = [GbaInput.B],
        [Rayman3Input.StorySkip] = [GbaInput.Start],
        [Rayman3Input.StoryNext] = [GbaInput.A],
        [Rayman3Input.TextBoxSkip] = [GbaInput.Start],
        [Rayman3Input.TextBoxNext] = [GbaInput.A],
        [Rayman3Input.CreditsSkip] = [GbaInput.A, GbaInput.B, GbaInput.Start],
        [Rayman3Input.Pause] = [GbaInput.Start],
        [Rayman3Input.GameOverContinue] = [GbaInput.A],
        [Rayman3Input.IntroSkip] = [GbaInput.Start],
        [Rayman3Input.ActorUp] = [GbaInput.Up],
        [Rayman3Input.ActorDown] = [GbaInput.Down],
        [Rayman3Input.ActorLeft] = [GbaInput.Left],
        [Rayman3Input.ActorRight] = [GbaInput.Right],
        [Rayman3Input.ActorJump] = [GbaInput.A],
        [Rayman3Input.ActorAttack] = [GbaInput.B],
        [Rayman3Input.ActorSpecialLeft] = [GbaInput.L],
        [Rayman3Input.ActorSpecialRight] = [GbaInput.R],
    }.ToFrozenDictionary();
    private static FrozenDictionary<Rayman3Input, GbaInput[]> Rayman3NGageInputs { get; } = new Dictionary<Rayman3Input, GbaInput[]>
    {
        [Rayman3Input.MenuUp] = [GbaInput.Up],
        [Rayman3Input.MenuDown] = [GbaInput.Down],
        [Rayman3Input.MenuLeft] = [GbaInput.Left],
        [Rayman3Input.MenuRight] = [GbaInput.Right],
        [Rayman3Input.MenuLeftExt] = [GbaInput.Left, GbaInput.L],
        [Rayman3Input.MenuRightExt] = [GbaInput.Right, GbaInput.R],
        [Rayman3Input.MenuConfirm] = [GbaInput.A, GbaInput.Start], // NOTE: The game also checks numpad 0
        [Rayman3Input.MenuBack] = [GbaInput.B, GbaInput.Select],
        [Rayman3Input.MenuLostMultiplayerConnectionBack] = [GbaInput.A, GbaInput.B, GbaInput.Start, GbaInput.Select], // NOTE: The game also checks numpad 0
        [Rayman3Input.MenuMultiplayerStart] = [],
        [Rayman3Input.PauseMenuBack] = [GbaInput.B, GbaInput.Select],
        [Rayman3Input.GameCubeMenuConfirm] = [],
        [Rayman3Input.GameCubeMenuBack] = [],
        [Rayman3Input.LevelSelectMenuConfirm] = [GbaInput.A],
        [Rayman3Input.LevelSelectMenuBack] = [GbaInput.B],
        [Rayman3Input.StorySkip] = [GbaInput.Start, GbaInput.Select],
        [Rayman3Input.StoryNext] = [GbaInput.A, GbaInput.B, GbaInput.L, GbaInput.R], // NOTE: The game also checks numpad 0-4 and 9
        [Rayman3Input.TextBoxSkip] = [GbaInput.Start],
        [Rayman3Input.TextBoxNext] = [GbaInput.A],
        [Rayman3Input.CreditsSkip] = [GbaInput.A, GbaInput.B, GbaInput.L, GbaInput.R, GbaInput.Start, GbaInput.Select], // NOTE: The game also checks numpad 0-4 and 9
        [Rayman3Input.Pause] = [GbaInput.Start, GbaInput.Select],
        [Rayman3Input.GameOverContinue] = [GbaInput.A, GbaInput.B],
        [Rayman3Input.IntroSkip] = [GbaInput.A, GbaInput.B, GbaInput.L, GbaInput.R, GbaInput.Start, GbaInput.Select], // NOTE: The game also checks numpad 0-4 and 9
        [Rayman3Input.ActorUp] = [GbaInput.Up],
        [Rayman3Input.ActorDown] = [GbaInput.Down],
        [Rayman3Input.ActorLeft] = [GbaInput.Left],
        [Rayman3Input.ActorRight] = [GbaInput.Right],
        [Rayman3Input.ActorJump] = [GbaInput.A],
        [Rayman3Input.ActorAttack] = [GbaInput.B],
        [Rayman3Input.ActorSpecialLeft] = [GbaInput.L],
        [Rayman3Input.ActorSpecialRight] = [GbaInput.R],
    }.ToFrozenDictionary();

    // Additional keyboard inputs to allow if they're not mapped (so you can pause with ESC etc.)
    private static FrozenDictionary<Rayman3Input, Keys[]> Rayman3StandardKeyboardInputs { get; } = new Dictionary<Rayman3Input, Keys[]>
    {
        [Rayman3Input.MenuUp] = [],
        [Rayman3Input.MenuDown] = [],
        [Rayman3Input.MenuLeft] = [],
        [Rayman3Input.MenuRight] = [],
        [Rayman3Input.MenuLeftExt] = [],
        [Rayman3Input.MenuRightExt] = [],
        [Rayman3Input.MenuConfirm] = [Keys.Enter, Keys.Space],
        [Rayman3Input.MenuBack] = [Keys.Escape, Keys.Back],
        [Rayman3Input.MenuLostMultiplayerConnectionBack] = [Keys.Enter, Keys.Space, Keys.Escape, Keys.Back],
        [Rayman3Input.MenuMultiplayerStart] = [Keys.Enter, Keys.Space],
        [Rayman3Input.PauseMenuBack] = [Keys.Escape, Keys.Back],
        [Rayman3Input.GameCubeMenuConfirm] = [Keys.Enter, Keys.Space],
        [Rayman3Input.GameCubeMenuBack] = [Keys.Escape, Keys.Back],
        [Rayman3Input.LevelSelectMenuConfirm] = [Keys.Enter, Keys.Space],
        [Rayman3Input.LevelSelectMenuBack] = [Keys.Escape, Keys.Back],
        [Rayman3Input.StorySkip] = [Keys.Escape],
        [Rayman3Input.StoryNext] = [Keys.Enter, Keys.Space],
        [Rayman3Input.TextBoxSkip] = [Keys.Escape],
        [Rayman3Input.TextBoxNext] = [Keys.Enter, Keys.Space],
        [Rayman3Input.CreditsSkip] = [Keys.Enter, Keys.Space, Keys.Escape, Keys.Back],
        [Rayman3Input.Pause] = [Keys.Escape],
        [Rayman3Input.GameOverContinue] = [Keys.Enter, Keys.Space, Keys.Escape, Keys.Back],
        [Rayman3Input.IntroSkip] = [Keys.Enter, Keys.Space, Keys.Escape, Keys.Back],
        [Rayman3Input.ActorUp] = [],
        [Rayman3Input.ActorDown] = [],
        [Rayman3Input.ActorLeft] = [],
        [Rayman3Input.ActorRight] = [],
        [Rayman3Input.ActorJump] = [],
        [Rayman3Input.ActorAttack] = [],
        [Rayman3Input.ActorSpecialLeft] = [],
        [Rayman3Input.ActorSpecialRight] = [],
    }.ToFrozenDictionary();

    private static GbaInput[] GetGbaInputs(Rayman3Input rayman3Input)
    {
        return Engine.Settings.Local.Controls.UseModernButtonMapping switch
        {
            true => Rayman3ModernInputs[rayman3Input],
            false when Rom.Platform is Platform.GBA => Rayman3GbaInputs[rayman3Input],
            false when Rom.Platform is Platform.NGage => Rayman3NGageInputs[rayman3Input],
            _ => throw new UnsupportedPlatformException()
        };
    }

    private static Keys[] GetKeyboardInputs(Rayman3Input rayman3Input)
    {
        return Rayman3StandardKeyboardInputs[rayman3Input];
    }

    extension(SimpleJoyPad simpleJoyPad)
    {
        // Checks if any button is pressed
        public bool IsButtonPressed(Rayman3Input rayman3Input)
        {
            GbaInput[] inputs = GetGbaInputs(rayman3Input);

            for (int i = 0; i < inputs.Length; i++)
            {
                if (simpleJoyPad.IsButtonPressed(inputs[i]))
                    return true;
            }

            if (Engine.Settings.Local.Controls.UseStandardKeyboardKeys && simpleJoyPad.ReceivedInputsFromUser)
            {
                Keys[] keys = GetKeyboardInputs(rayman3Input);

                for (int i = 0; i < keys.Length; i++)
                {
                    if (!Engine.Input.IsKeyMapped(keys[i]) && Engine.Input.IsKeyPressed(keys[i]))
                        return true;
                }
            }

            return false;
        }

        // Checks so all buttons are released
        public bool IsButtonReleased(Rayman3Input rayman3Input)
        {
            GbaInput[] inputs = GetGbaInputs(rayman3Input);

            for (int i = 0; i < inputs.Length; i++)
            {
                if (!simpleJoyPad.IsButtonReleased(inputs[i]))
                    return false;
            }

            if (Engine.Settings.Local.Controls.UseStandardKeyboardKeys && simpleJoyPad.ReceivedInputsFromUser)
            {
                Keys[] keys = GetKeyboardInputs(rayman3Input);

                for (int i = 0; i < keys.Length; i++)
                {
                    if (!Engine.Input.IsKeyMapped(keys[i]) && !Engine.Input.IsKeyReleased(keys[i]))
                        return false;
                }
            }

            return true;
        }

        // Checks if any button is just pressed
        public bool IsButtonJustPressed(Rayman3Input rayman3Input)
        {
            GbaInput[] inputs = GetGbaInputs(rayman3Input);

            for (int i = 0; i < inputs.Length; i++)
            {
                if (simpleJoyPad.IsButtonJustPressed(inputs[i]))
                    return true;
            }

            if (Engine.Settings.Local.Controls.UseStandardKeyboardKeys && simpleJoyPad.ReceivedInputsFromUser)
            {
                Keys[] keys = GetKeyboardInputs(rayman3Input);

                for (int i = 0; i < keys.Length; i++)
                {
                    if (!Engine.Input.IsKeyMapped(keys[i]) && Engine.Input.IsKeyJustPressed(keys[i]))
                        return true;
                }
            }

            return false;
        }

        // Checks if any button is just released
        public bool IsButtonJustReleased(Rayman3Input rayman3Input)
        {
            GbaInput[] inputs = GetGbaInputs(rayman3Input);

            for (int i = 0; i < inputs.Length; i++)
            {
                if (simpleJoyPad.IsButtonJustReleased(inputs[i]))
                    return true;
            }

            if (Engine.Settings.Local.Controls.UseStandardKeyboardKeys && simpleJoyPad.ReceivedInputsFromUser)
            {
                Keys[] keys = GetKeyboardInputs(rayman3Input);

                for (int i = 0; i < keys.Length; i++)
                {
                    if (!Engine.Input.IsKeyMapped(keys[i]) && Engine.Input.IsKeyJustReleased(keys[i]))
                        return true;
                }
            }

            return false;
        }
    }
}