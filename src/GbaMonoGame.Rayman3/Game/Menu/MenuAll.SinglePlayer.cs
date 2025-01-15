using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Properties

    public byte StartEraseCursorTargetIndex { get; set; }
    public byte StartEraseCursorCurrentIndex { get; set; }
    public byte EraseSaveStage { get; set; }

    #endregion

    #region Private Methods

    private void SetEraseCursorTargetIndex(byte targetIndex)
    {
        StartEraseCursorCurrentIndex = StartEraseCursorTargetIndex;
        StartEraseCursorTargetIndex = targetIndex;
    }

    private void MoveStartEraseCursor()
    {
        if (StartEraseCursorTargetIndex != StartEraseCursorCurrentIndex)
        {
            int targetXPos = Rom.Platform switch
            {
                Platform.GBA => StartEraseCursorTargetIndex * 72 + 106,
                Platform.NGage => StartEraseCursorTargetIndex * 72 + 78,
                _ => throw new UnsupportedPlatformException()
            };

            if (StartEraseCursorTargetIndex < StartEraseCursorCurrentIndex)
            {
                if (Data.StartEraseCursor.ScreenPos.X > targetXPos)
                {
                    Data.StartEraseCursor.ScreenPos -= new Vector2(4, 0);
                }
                else
                {
                    Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { X = targetXPos };
                    StartEraseCursorCurrentIndex = StartEraseCursorTargetIndex;
                }
            }
            else
            {
                if (Data.StartEraseCursor.ScreenPos.X < targetXPos)
                {
                    Data.StartEraseCursor.ScreenPos += new Vector2(4, 0);
                }
                else
                {
                    Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { X = targetXPos };
                    StartEraseCursorCurrentIndex = StartEraseCursorTargetIndex;
                }
            }
        }
    }

    #endregion

    #region Steps

    private void Step_InitializeTransitionToSinglePlayer()
    {
        foreach (SpriteTextObject slotLumText in Data.SlotLumTexts)
            slotLumText.Text = "1000";

        foreach (SpriteTextObject slotCageText in Data.SlotCageTexts)
            slotCageText.Text = "50";

        foreach (AnimatedObject slotEmptyText in Data.SlotEmptyTexts)
            slotEmptyText.CurrentAnimation = Localization.LanguageUiIndex;

        Data.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 1;

        if (Rom.Platform == Platform.GBA)
            Data.StartEraseCursor.CurrentAnimation = 40;

        for (int i = 0; i < 3; i++)
        {
            if (Slots[i] != null)
            {
                Data.SlotLumTexts[i].Text = Slots[i].LumsCount.ToString();
                Data.SlotCageTexts[i].Text = Slots[i].CagesCount.ToString();
            }
        }

        CurrentStepAction = Step_TransitionToSinglePlayer;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        ResetStem();
        SetBackgroundPalette(1);
        StartEraseCursorCurrentIndex = 0;
        StartEraseCursorTargetIndex = 0;
        EraseSaveStage = 0;

        if (Rom.Platform == Platform.GBA)
        {
            Data.StartEraseSelection.ScreenPos = new Vector2(80, 30);
            Data.StartEraseCursor.ScreenPos = new Vector2(106, 12);
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Data.StartEraseSelection.ScreenPos = new Vector2(52, 30);
            Data.StartEraseCursor.ScreenPos = new Vector2(78, 12);
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private void Step_TransitionToSinglePlayer()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position += new Vector2(0, 8);
        }

        Data.StartEraseSelection.ScreenPos = Data.StartEraseSelection.ScreenPos with { Y = TransitionValue / 2f - 50 };
        Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = TransitionValue / 2f - 68 };

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_SinglePlayer;
        }

        for (int i = 0; i < 3; i++)
        {
            AnimationPlayer.Play(Data.SlotIcons[i]);

            if (Slots[i] == null)
            {
                AnimationPlayer.Play(Data.SlotEmptyTexts[i]);
            }
            else
            {
                AnimationPlayer.Play(Data.SlotLumTexts[i]);
                AnimationPlayer.Play(Data.SlotCageTexts[i]);
                AnimationPlayer.Play(Data.SlotLumIcons[i]);
                AnimationPlayer.Play(Data.SlotCageIcons[i]);
            }
        }

        AnimationPlayer.Play(Data.StartEraseSelection);

        if (Rom.Platform == Platform.GBA)
            AnimationPlayer.Play(Data.StartEraseCursor);
    }

    private void Step_SinglePlayer()
    {
        switch (EraseSaveStage)
        {
            case 0:
                if (IsStartingGame)
                {
                    if (TransitionsFX.IsFadeOutFinished)
                    {
                        SoundEventsManager.StopAllSongs();

                        if (Slots[SelectedOption] == null)
                        {
                            // Create a new game
                            FrameManager.SetNextFrame(new Act1());
                            GameInfo.ResetPersistentInfo();
                        }
                        else
                        {
                            // Load an existing game
                            GameInfo.Load(SelectedOption);
                            GameInfo.LoadLastWorld();
                        }

                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = 1;

                        GameInfo.CurrentSlot = SelectedOption;
                        IsStartingGame = false;

                        if (Rom.Platform == Platform.GBA)
                            Engine.Config.LastPlayedGbaSaveSlot = GameInfo.CurrentSlot;
                        else if (Rom.Platform == Platform.NGage)
                            Engine.Config.LastPlayedNGageSaveSlot = GameInfo.CurrentSlot;
                        else
                            throw new UnsupportedPlatformException();
                    }
                }
                // Move start/erase to start
                else if ((JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.L)) && Data.Cursor.CurrentAnimation != 16)
                {
                    if (StartEraseCursorTargetIndex != 0)
                    {
                        SetEraseCursorTargetIndex(0);
                        Data.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 1;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move start/erase to erase
                else if ((JoyPad.IsButtonJustPressed(GbaInput.Right) || JoyPad.IsButtonJustPressed(GbaInput.R)) && Data.Cursor.CurrentAnimation != 16)
                {
                    if (StartEraseCursorTargetIndex != 1)
                    {
                        SetEraseCursorTargetIndex(1);
                        Data.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move up
                else if (JoyPad.IsButtonJustPressed(GbaInput.Up) && Data.Cursor.CurrentAnimation != 16)
                {
                    if (SelectedOption == 0)
                        SelectOption(2, true);
                    else
                        SelectOption(SelectedOption - 1, true);
                }
                // Move down
                else if (JoyPad.IsButtonJustPressed(GbaInput.Down) && Data.Cursor.CurrentAnimation != 16)
                {
                    if (SelectedOption == 2)
                        SelectOption(0, true);
                    else
                        SelectOption(SelectedOption + 1, true);
                }
                // Select slot
                else if (JoyPad.IsButtonJustPressed(GbaInput.A) && Data.Cursor.CurrentAnimation != 16)
                {
                    Data.Cursor.CurrentAnimation = 16;

                    if (StartEraseCursorTargetIndex != 1)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                    else if (Slots[SelectedOption] != null)
                    {
                        EraseSaveStage = 1;
                        TransitionValue = 0;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                    else
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    }
                }
                break;

            case 1:
                TransitionValue += 4;
                Data.StartEraseSelection.ScreenPos = Data.StartEraseSelection.ScreenPos with { Y = 30 - TransitionValue };
                Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    EraseSaveStage = 2;
                    Data.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 21;

                    if (Rom.Platform == Platform.GBA)
                        Data.StartEraseSelection.ScreenPos = new Vector2(144, -80);
                    else if (Rom.Platform == Platform.NGage)
                        Data.StartEraseSelection.ScreenPos = new Vector2(104, -80);
                    else
                        throw new UnsupportedPlatformException();

                    Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = -38 };
                }
                break;

            case 2:
                TransitionValue += 4;
                Data.StartEraseSelection.ScreenPos = Data.StartEraseSelection.ScreenPos with { Y = TransitionValue - 80 };
                Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = TransitionValue - 38 };

                if (TransitionValue >= 80)
                {
                    TransitionValue = 0;
                    EraseSaveStage = 3;
                }
                break;

            case 3:
                // Move left
                if (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.L))
                {
                    if (StartEraseCursorTargetIndex != 0)
                    {
                        SetEraseCursorTargetIndex(0);
                        Data.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 20;
                        // TODO: Game passes in 0 as obj here, but that's probably a mistake
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move right
                else if (JoyPad.IsButtonJustPressed(GbaInput.Right) || JoyPad.IsButtonJustPressed(GbaInput.R))
                {
                    if (StartEraseCursorTargetIndex != 1)
                    {
                        SetEraseCursorTargetIndex(1);
                        Data.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 21;
                        // TODO: Game passes in 0 as obj here, but that's probably a mistake
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Erase slot
                else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    EraseSaveStage = 4;
                    TransitionValue = 0;
                    if (StartEraseCursorTargetIndex == 0 && Slots[SelectedOption] != null)
                    {
                        Slots[SelectedOption] = null;
                        SaveGameManager.DeleteSlot(SelectedOption);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                }
                break;

            case 4:
                TransitionValue += 4;
                Data.StartEraseSelection.ScreenPos = Data.StartEraseSelection.ScreenPos with { Y = -TransitionValue };
                Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = 42 - TransitionValue };

                if (TransitionValue >= 80)
                {
                    TransitionValue = 0;
                    EraseSaveStage = 5;
                    Data.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;

                    if (Rom.Platform == Platform.GBA)
                        Data.StartEraseSelection.ScreenPos = new Vector2(80, -50);
                    else if (Rom.Platform == Platform.NGage)
                        Data.StartEraseSelection.ScreenPos = new Vector2(52, -50);
                    else
                        throw new UnsupportedPlatformException();

                    Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = -68 };
                }
                break;

            case 5:
                TransitionValue += 4;
                Data.StartEraseSelection.ScreenPos = Data.StartEraseSelection.ScreenPos with { Y = TransitionValue - 34 };
                Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = TransitionValue - 52 };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    EraseSaveStage = 0;
                }
                break;
        }

        if (JoyPad.IsButtonJustPressed(GbaInput.B) && TransitionsFX.IsFadeOutFinished && !IsStartingGame)
        {
            if (EraseSaveStage == 0)
            {
                NextStepAction = Step_InitializeTransitionToSelectGameMode;
                CurrentStepAction = Step_TransitionOutOfSinglePlayer;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                TransitionValue = 0;
                SelectOption(0, false);
                TransitionOutCursorAndStem();
            }
            else if (EraseSaveStage == 1)
            {
                EraseSaveStage = 5;
            }
            else if (EraseSaveStage == 2)
            {
                EraseSaveStage = 4;
                TransitionValue = 80 - TransitionValue;
            }
            else if (EraseSaveStage == 3)
            {
                EraseSaveStage = 4;
                TransitionValue = 0;
            }
        }

        MoveStartEraseCursor();

        for (int i = 0; i < 3; i++)
        {
            AnimationPlayer.Play(Data.SlotIcons[i]);

            if (Slots[i] == null)
            {
                AnimationPlayer.Play(Data.SlotEmptyTexts[i]);
            }
            else
            {
                AnimationPlayer.Play(Data.SlotLumTexts[i]);
                AnimationPlayer.Play(Data.SlotCageTexts[i]);
                AnimationPlayer.Play(Data.SlotLumIcons[i]);
                AnimationPlayer.Play(Data.SlotCageIcons[i]);
            }
        }

        AnimationPlayer.Play(Data.StartEraseSelection);
        AnimationPlayer.Play(Data.StartEraseCursor);

        if (!IsStartingGame && Data.Cursor.CurrentAnimation == 16 && Data.Cursor.EndOfAnimation)
        {
            Data.Cursor.CurrentAnimation = 0;

            if (StartEraseCursorTargetIndex == 0)
            {
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                IsStartingGame = true;
                TransitionsFX.FadeOutInit(2 / 16f);
            }
        }
    }

    private void Step_TransitionOutOfSinglePlayer()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position -= new Vector2(0, 4);
            Data.StartEraseSelection.ScreenPos = Data.StartEraseSelection.ScreenPos with { Y = 30 - TransitionValue / 2f };
            Data.StartEraseCursor.ScreenPos = Data.StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue / 2f };
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        for (int i = 0; i < 3; i++)
        {
            AnimationPlayer.Play(Data.SlotIcons[i]);

            if (Slots[i] == null)
            {
                AnimationPlayer.Play(Data.SlotEmptyTexts[i]);
            }
            else
            {
                AnimationPlayer.Play(Data.SlotLumTexts[i]);
                AnimationPlayer.Play(Data.SlotCageTexts[i]);
                AnimationPlayer.Play(Data.SlotLumIcons[i]);
                AnimationPlayer.Play(Data.SlotCageIcons[i]);
            }
        }

        AnimationPlayer.Play(Data.StartEraseSelection);
        AnimationPlayer.Play(Data.StartEraseCursor);
    }

    #endregion
}