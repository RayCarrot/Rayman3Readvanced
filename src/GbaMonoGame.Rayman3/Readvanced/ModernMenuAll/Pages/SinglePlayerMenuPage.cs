using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class SinglePlayerMenuPage : MenuPage
{
    public SinglePlayerMenuPage(ModernMenuAll menu) : base(menu) { }

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 18;

    public AnimatedObject StartEraseSelection { get; set; }
    public AnimatedObject StartEraseCursor { get; set; }

    public byte StartEraseCursorTargetIndex { get; set; }
    public byte StartEraseCursorCurrentIndex { get; set; }
    public StartEraseMode EraseSaveStage { get; set; }
    public bool IsStartingGame { get; set; }

    private void SetEraseCursorTargetIndex(byte targetIndex)
    {
        StartEraseCursorCurrentIndex = StartEraseCursorTargetIndex;
        StartEraseCursorTargetIndex = targetIndex;
    }

    private void MoveStartEraseCursor()
    {
        if (StartEraseCursorTargetIndex != StartEraseCursorCurrentIndex)
        {
            int targetXPos = StartEraseCursorTargetIndex * 72 + 136;

            if (StartEraseCursorTargetIndex < StartEraseCursorCurrentIndex)
            {
                if (StartEraseCursor.ScreenPos.X > targetXPos)
                {
                    StartEraseCursor.ScreenPos -= new Vector2(4, 0);
                }
                else
                {
                    StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { X = targetXPos };
                    StartEraseCursorCurrentIndex = StartEraseCursorTargetIndex;
                }
            }
            else
            {
                if (StartEraseCursor.ScreenPos.X < targetXPos)
                {
                    StartEraseCursor.ScreenPos += new Vector2(4, 0);
                }
                else
                {
                    StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { X = targetXPos };
                    StartEraseCursorCurrentIndex = StartEraseCursorTargetIndex;
                }
            }
        }
    }

    protected override void Init()
    {
        // Add slots
        for (int i = 0; i < Menu.Slots.Length; i++)
            AddOption(new SlotMenuOption());

        // Create animations
        AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);

        StartEraseSelection = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(110, 30),
            CurrentAnimation = 1,
            RenderContext = RenderContext,
        };

        StartEraseCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(136, 12),
            CurrentAnimation = 40,
            RenderContext = RenderContext,
        };

        // Reset values
        StartEraseCursorCurrentIndex = 0;
        StartEraseCursorTargetIndex = 0;
        EraseSaveStage = StartEraseMode.Selection;
    }

    protected override void Step_TransitionIn()
    {
        StartEraseSelection.ScreenPos = StartEraseSelection.ScreenPos with { Y = TransitionValue / 2f - 50 };
        StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = TransitionValue / 2f - 68 };
    }

    protected override void Step_Active()
    {
        switch (EraseSaveStage)
        {
            case StartEraseMode.Selection:
                if (IsStartingGame)
                {
                    if (Menu.TransitionsFX.IsFadeOutFinished)
                    {
                        SoundEventsManager.StopAllSongs();

                        if (Menu.Slots[SelectedOption] == null)
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
                else if ((JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.L)) && Menu.Cursor.CurrentAnimation != 16)
                {
                    if (StartEraseCursorTargetIndex != 0)
                    {
                        SetEraseCursorTargetIndex(0);
                        StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 1;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move start/erase to erase
                else if ((JoyPad.IsButtonJustPressed(GbaInput.Right) || JoyPad.IsButtonJustPressed(GbaInput.R)) && Menu.Cursor.CurrentAnimation != 16)
                {
                    if (StartEraseCursorTargetIndex != 1)
                    {
                        SetEraseCursorTargetIndex(1);
                        StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move up
                else if (JoyPad.IsButtonJustPressed(GbaInput.Up) && Menu.Cursor.CurrentAnimation != 16)
                {
                    ChangeSelectedOption(-1);
                }
                // Move down
                else if (JoyPad.IsButtonJustPressed(GbaInput.Down) && Menu.Cursor.CurrentAnimation != 16)
                {
                    ChangeSelectedOption(1);
                }
                // Select slot
                else if (JoyPad.IsButtonJustPressed(GbaInput.A) && Menu.Cursor.CurrentAnimation != 16)
                {
                    Menu.Cursor.CurrentAnimation = 16;

                    if (StartEraseCursorTargetIndex != 1)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                    else if (Menu.Slots[SelectedOption] != null)
                    {
                        EraseSaveStage = StartEraseMode.TransitionOutSelection;
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
                StartEraseSelection.ScreenPos = StartEraseSelection.ScreenPos with { Y = 30 - TransitionValue };
                StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    EraseSaveStage = StartEraseMode.TransitionInConfirmErase;
                    StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 21;
                    StartEraseSelection.ScreenPos = new Vector2(174, -80);
                    StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = -38 };
                }
                break;

            case StartEraseMode.TransitionInConfirmErase:
                TransitionValue += 4;
                StartEraseSelection.ScreenPos = StartEraseSelection.ScreenPos with { Y = TransitionValue - 80 };
                StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = TransitionValue - 38 };

                if (TransitionValue >= 80)
                {
                    TransitionValue = 0;
                    EraseSaveStage = StartEraseMode.ConfirmErase;
                }
                break;

            case StartEraseMode.ConfirmErase:
                // Move left
                if (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.L))
                {
                    if (StartEraseCursorTargetIndex != 0)
                    {
                        SetEraseCursorTargetIndex(0);
                        StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 20;
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
                        StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + 21;
                        // TODO: Game passes in 0 as obj here, but that's probably a mistake
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Erase slot
                else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    EraseSaveStage = StartEraseMode.TransitionOutConfirmErase;
                    TransitionValue = 0;
                    if (StartEraseCursorTargetIndex == 0 && Menu.Slots[SelectedOption] != null)
                    {
                        Menu.Slots[SelectedOption] = null;
                        ((SlotMenuOption)Options[SelectedOption]).Slot = null;
                        SaveGameManager.DeleteSlot(SelectedOption);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                }
                break;

            case StartEraseMode.TransitionOutConfirmErase:
                TransitionValue += 4;
                StartEraseSelection.ScreenPos = StartEraseSelection.ScreenPos with { Y = -TransitionValue };
                StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = 42 - TransitionValue };

                if (TransitionValue >= 80)
                {
                    TransitionValue = 0;
                    EraseSaveStage = StartEraseMode.TransitionInSelection;
                    StartEraseSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;
                    StartEraseSelection.ScreenPos = new Vector2(110, -50);
                    StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = -68 };
                }
                break;

            case StartEraseMode.TransitionInSelection:
                TransitionValue += 4;
                StartEraseSelection.ScreenPos = StartEraseSelection.ScreenPos with { Y = TransitionValue - 34 };
                StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = TransitionValue - 52 };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    EraseSaveStage = StartEraseMode.Selection;
                }
                break;
        }

        if (JoyPad.IsButtonJustPressed(GbaInput.B) && Menu.TransitionsFX.IsFadeOutFinished && !IsStartingGame)
        {
            switch (EraseSaveStage)
            {
                case StartEraseMode.Selection:
                    Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Back);
                    break;
                
                case StartEraseMode.TransitionOutSelection:
                    EraseSaveStage = StartEraseMode.TransitionInSelection;
                    break;
                
                case StartEraseMode.TransitionInConfirmErase:
                    EraseSaveStage = StartEraseMode.TransitionOutConfirmErase;
                    TransitionValue = 80 - TransitionValue;
                    break;
                
                case StartEraseMode.ConfirmErase:
                    EraseSaveStage = StartEraseMode.TransitionOutConfirmErase;
                    TransitionValue = 0;
                    break;
            }
        }

        MoveStartEraseCursor();

        if (!IsStartingGame && Menu.Cursor.CurrentAnimation == 16 && Menu.Cursor.EndOfAnimation)
        {
            Menu.Cursor.CurrentAnimation = 0;

            if (StartEraseCursorTargetIndex == 0)
            {
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                IsStartingGame = true;
                Menu.TransitionsFX.FadeOutInit(2 / 16f);
            }
        }
    }

    protected override void Step_TransitionOut()
    {
        StartEraseSelection.ScreenPos = StartEraseSelection.ScreenPos with { Y = 30 - TransitionValue / 2f };
        StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue / 2f };
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
        animationPlayer.Play(StartEraseSelection);
        animationPlayer.Play(StartEraseCursor);
    }
}