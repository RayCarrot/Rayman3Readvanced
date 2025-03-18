using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class ModernPauseDialog
{
    public bool Fsm_CheckSelection(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SetOptions(
                [
                    "CONTINUE",
                    "OPTIONS",
                    CanExitLevel ? "EXIT LEVEL" : "QUIT GAME",
                ]);
                SetSelectedOption(0);
                break;

            case FsmAction.Step:
                bool resume = false;
                bool options = false;
                bool quitGame = false;

                if (JoyPad.IsButtonJustPressed(GbaInput.Up))
                {
                    SetSelectedOption(SelectedOption - 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
                {
                    SetSelectedOption(SelectedOption + 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.B) || JoyPad.IsButtonJustPressed(GbaInput.Start))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    DrawStep = PauseDialogDrawStep.MoveOut;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

                    if (Rom.Platform == Platform.NGage)
                        ((NGageSoundEventsManager)SoundEventsManager.Current).ResumeLoopingSoundEffects();

                    resume = true;
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);

                    switch (SelectedOption)
                    {
                        // Continue
                        case 0:
                            DrawStep = PauseDialogDrawStep.MoveOut;
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

                            if (Rom.Platform == Platform.NGage)
                                ((NGageSoundEventsManager)SoundEventsManager.Current).ResumeLoopingSoundEffects();

                            resume = true;
                            break;

                        // Options
                        case 1:
                            options = true;
                            break;

                        // Exit level / quit game
                        case 2:
                            quitGame = true;
                            break;
                    }
                }

                if (resume)
                {
                    State.MoveTo(null);
                    return false;
                }

                if (options)
                {
                    // TODO: Implement
                    return false;
                }

                if (quitGame)
                {
                    State.MoveTo(Fsm_QuitGame);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_QuitGame(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SetOptions(
                [
                    "YES",
                    "NO",
                ]);
                SetSelectedOption(1);
                break;

            case FsmAction.Step:
                bool goBack = false;

                if (JoyPad.IsButtonJustPressed(GbaInput.Up))
                {
                    SetSelectedOption(SelectedOption - 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
                {
                    SetSelectedOption(SelectedOption + 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.B) || (JoyPad.IsButtonJustPressed(GbaInput.A) && SelectedOption == 1))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    goBack = true;
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.A) && SelectedOption == 0)
                {
                    // Exit level
                    if (CanExitLevel)
                    {
                        DrawStep = PauseDialogDrawStep.MoveOut;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

                        if (Rom.Platform == Platform.NGage)
                            ((NGageSoundEventsManager)SoundEventsManager.Current).ResumeLoopingSoundEffects();

                        State.MoveTo(null);
                        
                        // TODO: Implement exit level message for the Mode7 actors
                        Scene.MainActor.ProcessMessage(this, Message.Main_LevelExit);
                    }
                    // Quit game
                    else
                    {
                        GameTime.Resume();

                        SoundEventsManager.StopAllSongs();
                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = 1;

                        if (Rom.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                            FrameManager.SetNextFrame(new GameCubeMenu());
                        else
                            FrameManager.SetNextFrame(new ModernMenuAll(InitialMenuPage.GameMode));
                    }

                    if (Rom.Platform == Platform.GBA)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                }

                if (goBack)
                {
                    State.MoveTo(Fsm_CheckSelection);
                    SetSelectedOption(2);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}