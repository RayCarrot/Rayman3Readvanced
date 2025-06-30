﻿using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

// N-Gage exclusive
public partial class MenuAll
{
    #region Steps

    private void Step_InitializeTransitionToQuit()
    {
        SelectOption(0, false);

        Anims.QuitSelection.CurrentAnimation = 15 + Localization.LanguageUiIndex + SelectedOption * 5;
        Anims.QuitHeader.CurrentAnimation = 34 + Localization.LanguageUiIndex;
        Anims.GameLogo.CurrentAnimation = 0;

        CurrentStepAction = Step_TransitionToQuit;
        ResetStem();
        SetBackgroundPalette(3);
    }

    private void Step_TransitionToQuit()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position += new Vector2(0, 8);
        }

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_Quit;
        }

        MoveGameLogo();

        // NOTE The game gives the render box a height of 255 instead of 240 here
        Anims.GameLogo.FrameChannelSprite();

        AnimationPlayer.Play(Anims.GameLogo);
        AnimationPlayer.Play(Anims.QuitSelection);
        AnimationPlayer.Play(Anims.QuitHeader);
    }

    private void Step_Quit()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up))
        {
            SelectOption(SelectedOption == 0 ? 1 : 0, true);
            Anims.QuitSelection.CurrentAnimation = 15 + Localization.LanguageUiIndex + SelectedOption * 5;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SelectOption(SelectedOption == 1 ? 0 : 1, true);
            Anims.QuitSelection.CurrentAnimation = 15 + Localization.LanguageUiIndex + SelectedOption * 5;
        }
        else if (NGageJoyPadHelpers.IsConfirmButtonJustPressed())
        {
            Anims.Cursor.CurrentAnimation = 16;

            if (SelectedOption == 0)
                NextStepAction = Step_InitializeTransitionToGameMode;
            else if (SelectedOption == 1)
                Engine.GbaGame.Exit();

            CurrentStepAction = Step_TransitionOutOfQuit;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
            SelectOption(0, false);
            TransitionValue = 0;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
            TransitionOutCursorAndStem();
        }
        else if (NGageJoyPadHelpers.IsBackButtonJustPressed())
        {
            Anims.Cursor.CurrentAnimation = 16;
            NextStepAction = Step_InitializeTransitionToGameMode;
            CurrentStepAction = Step_TransitionOutOfQuit;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
            SelectOption(0, false);
            TransitionValue = 0;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
            TransitionOutCursorAndStem();
        }

        AnimationPlayer.Play(Anims.QuitSelection);
        AnimationPlayer.Play(Anims.QuitHeader);

        MoveGameLogo();

        // NOTE The game gives the render box a height of 255 instead of 240 here
        Anims.GameLogo.FrameChannelSprite();

        AnimationPlayer.Play(Anims.GameLogo);
    }

    private void Step_TransitionOutOfQuit()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position -= new Vector2(0, 4);

            Anims.GameLogo.ScreenPos = Anims.GameLogo.ScreenPos with { Y = 16 - TransitionValue / 2f};
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        AnimationPlayer.Play(Anims.QuitSelection);

        MoveGameLogo();

        // NOTE The game gives the render box a height of 255 instead of 240 here
        Anims.GameLogo.FrameChannelSprite();

        AnimationPlayer.Play(Anims.GameLogo);
    }

    #endregion
}