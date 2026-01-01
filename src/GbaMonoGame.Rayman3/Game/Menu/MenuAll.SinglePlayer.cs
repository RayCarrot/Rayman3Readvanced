using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Properties

    public byte PrevSelectedStartEraseOption { get; set; }
    public byte SelectedStartEraseOption { get; set; }
    public StartEraseMode StartEraseMode { get; set; }

    #endregion

    #region Private Methods

    private void SelectStartEraseOption(byte targetIndex)
    {
        PrevSelectedStartEraseOption = SelectedStartEraseOption;
        SelectedStartEraseOption = targetIndex;
    }

    private void ManageStartEraseCursor()
    {
        if (SelectedStartEraseOption != PrevSelectedStartEraseOption)
        {
            int targetXPos = Rom.Platform switch
            {
                Platform.GBA => SelectedStartEraseOption * 72 + 106,
                Platform.NGage => SelectedStartEraseOption * 72 + 78,
                _ => throw new UnsupportedPlatformException()
            };

            if (SelectedStartEraseOption < PrevSelectedStartEraseOption)
            {
                if (Anims.StartEraseCursor.ScreenPos.X > targetXPos)
                {
                    Anims.StartEraseCursor.ScreenPos -= new Vector2(4, 0);
                }
                else
                {
                    Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { X = targetXPos };
                    PrevSelectedStartEraseOption = SelectedStartEraseOption;
                }
            }
            else
            {
                if (Anims.StartEraseCursor.ScreenPos.X < targetXPos)
                {
                    Anims.StartEraseCursor.ScreenPos += new Vector2(4, 0);
                }
                else
                {
                    Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { X = targetXPos };
                    PrevSelectedStartEraseOption = SelectedStartEraseOption;
                }
            }
        }
    }

    #endregion

    #region Steps

    private void Step_InitializeTransitionToSinglePlayer()
    {
        foreach (SpriteTextObject slotLumText in Anims.SlotLumTexts)
            slotLumText.Text = "1000";

        foreach (SpriteTextObject slotCageText in Anims.SlotCageTexts)
            slotCageText.Text = "50";

        foreach (AnimatedObject slotEmptyText in Anims.SlotEmptyTexts)
            slotEmptyText.CurrentAnimation = Localization.LanguageUiIndex;

        Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 1;

        if (Rom.Platform == Platform.GBA)
            Anims.StartEraseCursor.CurrentAnimation = 40;

        for (int i = 0; i < GameInfo.OriginalSaveSlotsCount; i++)
        {
            if (Slots[i] != null)
            {
                Anims.SlotLumTexts[i].Text = Slots[i].LumsCount.ToString();
                Anims.SlotCageTexts[i].Text = Slots[i].CagesCount.ToString();
            }
        }

        CurrentStepAction = Step_TransitionToSinglePlayer;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        ResetStem();
        SetBackgroundPalette(1);
        PrevSelectedStartEraseOption = 0;
        SelectedStartEraseOption = 0;
        StartEraseMode = StartEraseMode.Selection;

        if (Rom.Platform == Platform.GBA)
        {
            Anims.StartEraseSelection.ScreenPos = new Vector2(80, 30);
            Anims.StartEraseCursor.ScreenPos = new Vector2(106, 12);
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Anims.StartEraseSelection.ScreenPos = new Vector2(52, 30);
            Anims.StartEraseCursor.ScreenPos = new Vector2(78, 12);
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

        Anims.StartEraseSelection.ScreenPos = Anims.StartEraseSelection.ScreenPos with { Y = TransitionValue / 2f - 50 };
        Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = TransitionValue / 2f - 68 };

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_SinglePlayer;
        }

        for (int i = 0; i < GameInfo.OriginalSaveSlotsCount; i++)
        {
            AnimationPlayer.Play(Anims.SlotIcons[i]);

            if (Slots[i] == null)
            {
                AnimationPlayer.Play(Anims.SlotEmptyTexts[i]);
            }
            else
            {
                AnimationPlayer.Play(Anims.SlotLumTexts[i]);
                AnimationPlayer.Play(Anims.SlotCageTexts[i]);
                AnimationPlayer.Play(Anims.SlotLumIcons[i]);
                AnimationPlayer.Play(Anims.SlotCageIcons[i]);
            }
        }

        AnimationPlayer.Play(Anims.StartEraseSelection);

        if (Rom.Platform == Platform.GBA)
            AnimationPlayer.Play(Anims.StartEraseCursor);
    }

    private void Step_SinglePlayer()
    {
        switch (StartEraseMode)
        {
            case StartEraseMode.Selection:
                if (IsStartingGame)
                {
                    if (!TransitionsFX.IsFadingOut)
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
                            GameInfo.GotoLastSaveGame();
                        }

                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = 1;

                        GameInfo.CurrentSlot = SelectedOption;
                        IsStartingGame = false;
                    }
                }
                // Move start/erase to start
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeftExt) && Anims.Cursor.CurrentAnimation != 16)
                {
                    if (SelectedStartEraseOption != 0)
                    {
                        SelectStartEraseOption(0);
                        Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 1;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move start/erase to erase
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuRightExt) && Anims.Cursor.CurrentAnimation != 16)
                {
                    if (SelectedStartEraseOption != 1)
                    {
                        SelectStartEraseOption(1);
                        Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move up
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp) && Anims.Cursor.CurrentAnimation != 16)
                {
                    if (SelectedOption == 0)
                        SelectOption(2, true);
                    else
                        SelectOption(SelectedOption - 1, true);
                }
                // Move down
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown) && Anims.Cursor.CurrentAnimation != 16)
                {
                    if (SelectedOption == 2)
                        SelectOption(0, true);
                    else
                        SelectOption(SelectedOption + 1, true);
                }
                // Select slot
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm) && Anims.Cursor.CurrentAnimation != 16)
                {
                    Anims.Cursor.CurrentAnimation = 16;

                    if (SelectedStartEraseOption != 1)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                    else if (Slots[SelectedOption] != null)
                    {
                        StartEraseMode = StartEraseMode.TransitionOutSelection;
                        TransitionValue = 0;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                    else
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    }
                }
                break;

            case StartEraseMode.TransitionOutSelection:
                TransitionValue += 4;
                Anims.StartEraseSelection.ScreenPos = Anims.StartEraseSelection.ScreenPos with { Y = 30 - TransitionValue };
                Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    StartEraseMode = StartEraseMode.TransitionInConfirmErase;
                    Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 21;

                    if (Rom.Platform == Platform.GBA)
                        Anims.StartEraseSelection.ScreenPos = new Vector2(144, -80);
                    else if (Rom.Platform == Platform.NGage)
                        Anims.StartEraseSelection.ScreenPos = new Vector2(104, -80);
                    else
                        throw new UnsupportedPlatformException();

                    Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = -38 };
                }
                break;

            case StartEraseMode.TransitionInConfirmErase:
                TransitionValue += 4;
                Anims.StartEraseSelection.ScreenPos = Anims.StartEraseSelection.ScreenPos with { Y = TransitionValue - 80 };
                Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = TransitionValue - 38 };

                if (TransitionValue >= 80)
                {
                    TransitionValue = 0;
                    StartEraseMode = StartEraseMode.ConfirmErase;
                }
                break;

            case StartEraseMode.ConfirmErase:
                // Move left
                if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeftExt))
                {
                    if (SelectedStartEraseOption != 0)
                    {
                        SelectStartEraseOption(0);
                        Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 20;
                        // NOTE: The game mistakenly passes in 0 as obj here, but nothing happens since pan and roll-off aren't enabled for this event
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move right
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuRightExt))
                {
                    if (SelectedStartEraseOption != 1)
                    {
                        SelectStartEraseOption(1);
                        Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 21;
                        // NOTE: The game mistakenly passes in 0 as obj here, but nothing happens since pan and roll-off aren't enabled for this event
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Erase slot
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                {
                    StartEraseMode = StartEraseMode.TransitionOutConfirmErase;
                    TransitionValue = 0;
                    if (SelectedStartEraseOption == 0 && Slots[SelectedOption] != null)
                    {
                        Slots[SelectedOption] = null;
                        SaveGameManager.DeleteSlot(SelectedOption);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                }
                break;

            case StartEraseMode.TransitionOutConfirmErase:
                TransitionValue += 4;
                Anims.StartEraseSelection.ScreenPos = Anims.StartEraseSelection.ScreenPos with { Y = -TransitionValue };
                Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = 42 - TransitionValue };

                if (TransitionValue >= 80)
                {
                    TransitionValue = 0;
                    StartEraseMode = StartEraseMode.TransitionInSelection;

                    // Optionally fix a bug of the wrong animation being shown after erasing a slot
                    if (SelectedStartEraseOption == 1 || !Engine.LocalConfig.Tweaks.FixBugs)
                        Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;
                    else
                        Anims.StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 1;

                    if (Rom.Platform == Platform.GBA)
                        Anims.StartEraseSelection.ScreenPos = new Vector2(80, -50);
                    else if (Rom.Platform == Platform.NGage)
                        Anims.StartEraseSelection.ScreenPos = new Vector2(52, -50);
                    else
                        throw new UnsupportedPlatformException();

                    Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = -68 };
                }
                break;

            case StartEraseMode.TransitionInSelection:
                TransitionValue += 4;
                Anims.StartEraseSelection.ScreenPos = Anims.StartEraseSelection.ScreenPos with { Y = TransitionValue - 34 };
                Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = TransitionValue - 52 };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    StartEraseMode = StartEraseMode.Selection;
                }
                break;
        }

        if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack) && !TransitionsFX.IsFadingOut && !IsStartingGame)
        {
            switch (StartEraseMode)
            {
                case StartEraseMode.Selection:
                    NextStepAction = Step_InitializeTransitionToGameMode;
                    CurrentStepAction = Step_TransitionOutOfSinglePlayer;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                    TransitionValue = 0;
                    SelectOption(0, false);
                    TransitionOutCursorAndStem();
                    break;

                case StartEraseMode.TransitionOutSelection:
                    StartEraseMode = StartEraseMode.TransitionInSelection;
                    break;

                case StartEraseMode.TransitionInConfirmErase:
                    StartEraseMode = StartEraseMode.TransitionOutConfirmErase;
                    TransitionValue = 80 - TransitionValue;
                    break;

                case StartEraseMode.ConfirmErase:
                    StartEraseMode = StartEraseMode.TransitionOutConfirmErase;
                    TransitionValue = 0;
                    break;
            }
        }

        ManageStartEraseCursor();

        for (int i = 0; i < GameInfo.OriginalSaveSlotsCount; i++)
        {
            AnimationPlayer.Play(Anims.SlotIcons[i]);

            if (Slots[i] == null)
            {
                AnimationPlayer.Play(Anims.SlotEmptyTexts[i]);
            }
            else
            {
                AnimationPlayer.Play(Anims.SlotLumTexts[i]);
                AnimationPlayer.Play(Anims.SlotCageTexts[i]);
                AnimationPlayer.Play(Anims.SlotLumIcons[i]);
                AnimationPlayer.Play(Anims.SlotCageIcons[i]);
            }
        }

        AnimationPlayer.Play(Anims.StartEraseSelection);
        AnimationPlayer.Play(Anims.StartEraseCursor);

        if (!IsStartingGame && Anims.Cursor.CurrentAnimation == 16 && Anims.Cursor.EndOfAnimation)
        {
            Anims.Cursor.CurrentAnimation = 0;

            if (SelectedStartEraseOption == 0)
            {
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                IsStartingGame = true;
                TransitionsFX.FadeOutInit(2);
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
            Anims.StartEraseSelection.ScreenPos = Anims.StartEraseSelection.ScreenPos with { Y = 30 - TransitionValue / 2f };
            Anims.StartEraseCursor.ScreenPos = Anims.StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue / 2f };
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        for (int i = 0; i < GameInfo.OriginalSaveSlotsCount; i++)
        {
            AnimationPlayer.Play(Anims.SlotIcons[i]);

            if (Slots[i] == null)
            {
                AnimationPlayer.Play(Anims.SlotEmptyTexts[i]);
            }
            else
            {
                AnimationPlayer.Play(Anims.SlotLumTexts[i]);
                AnimationPlayer.Play(Anims.SlotCageTexts[i]);
                AnimationPlayer.Play(Anims.SlotLumIcons[i]);
                AnimationPlayer.Play(Anims.SlotCageIcons[i]);
            }
        }

        AnimationPlayer.Play(Anims.StartEraseSelection);
        AnimationPlayer.Play(Anims.StartEraseCursor);
    }

    #endregion
}