using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class SinglePlayerMenuPage : MenuPage
{
    public SinglePlayerMenuPage(ModernMenuAll menu) : base(menu) { }

    private const float TabHeaderWidth = 72;
    private const float TabsCursorMoveTime = 18;

    private static readonly string[] _readvancedStartEraseSelectionTexturePaths =
    [
        Assets.Menu.SaveSelectionOptions_0_English_0,
        Assets.Menu.SaveSelectionOptions_0_English_1,
        Assets.Menu.SaveSelectionOptions_0_English_2,
        Assets.Menu.SaveSelectionOptions_1_French_0,
        Assets.Menu.SaveSelectionOptions_1_French_1,
        Assets.Menu.SaveSelectionOptions_1_French_2,
        Assets.Menu.SaveSelectionOptions_2_Spanish_0,
        Assets.Menu.SaveSelectionOptions_2_Spanish_1,
        Assets.Menu.SaveSelectionOptions_2_Spanish_2,
        Assets.Menu.SaveSelectionOptions_3_German_0,
        Assets.Menu.SaveSelectionOptions_3_German_1,
        Assets.Menu.SaveSelectionOptions_3_German_2,
        Assets.Menu.SaveSelectionOptions_4_Italian_0,
        Assets.Menu.SaveSelectionOptions_4_Italian_1,
        Assets.Menu.SaveSelectionOptions_4_Italian_2,
        Assets.Menu.SaveSelectionOptions_5_Dutch_0,
        Assets.Menu.SaveSelectionOptions_5_Dutch_1,
        Assets.Menu.SaveSelectionOptions_5_Dutch_2,
        Assets.Menu.SaveSelectionOptions_6_Swedish_0,
        Assets.Menu.SaveSelectionOptions_6_Swedish_1,
        Assets.Menu.SaveSelectionOptions_6_Swedish_2,
        Assets.Menu.SaveSelectionOptions_7_Finnish_0,
        Assets.Menu.SaveSelectionOptions_7_Finnish_1,
        Assets.Menu.SaveSelectionOptions_7_Finnish_2,
        Assets.Menu.SaveSelectionOptions_8_Norwegian_0,
        Assets.Menu.SaveSelectionOptions_8_Norwegian_1,
        Assets.Menu.SaveSelectionOptions_8_Norwegian_2,
        Assets.Menu.SaveSelectionOptions_9_Danish_0,
        Assets.Menu.SaveSelectionOptions_9_Danish_1,
        Assets.Menu.SaveSelectionOptions_9_Danish_2,
    ];

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 20;

    public int CursorBaseX => EraseSaveStage is 
        StartEraseMode.TransitionInConfirmErase or 
        StartEraseMode.ConfirmErase or 
        StartEraseMode.TransitionOutConfirmErase 
        ? 136
        : 101;

    public AnimatedObject StartEraseSelection { get; set; }
    public SpriteTextureObject ReadvancedStartEraseSelection { get; set; }
    public AnimatedObject StartEraseCursor { get; set; }

    public float StartEraseCursorX { get; set; }
    public float? StartEraseCursorStartX { get; set; }
    public float? StartEraseCursorDestX { get; set; }

    public int SelectedStartEraseOption { get; set; }
    public StartEraseMode EraseSaveStage { get; set; }

    private void SetStartEraseCursorMovement(float startX, float endX)
    {
        StartEraseCursorStartX = startX;
        StartEraseCursorDestX = endX;
    }

    private void SelectStartEraseOption(int index)
    {
        SetStartEraseCursorMovement(StartEraseCursorX, index * TabHeaderWidth);
        SelectedStartEraseOption = index;
    }

    private void SetReadvancedStartEraseSelectionTexture(int index)
    {
        string path = _readvancedStartEraseSelectionTexturePaths[Rayman3.Loc.LanguageUiIndex * 3 + index];
        ReadvancedStartEraseSelection.Texture = Engine.Assets.FrameContentManager.Load<Texture2D>(path);
    }

    private void ManageStartEraseCursor()
    {
        if (StartEraseCursorStartX == null || StartEraseCursorDestX == null)
            return;

        float startX = StartEraseCursorStartX.Value;
        float destX = StartEraseCursorDestX.Value;

        // Move with a speed based on the distance
        float dist = destX - startX;
        float speed = dist / TabsCursorMoveTime;

        // Move
        if ((destX < startX && StartEraseCursorX > destX) ||
            (destX > startX && StartEraseCursorX < destX))
        {
            StartEraseCursorX += speed;
        }
        // Finished moving
        else
        {
            StartEraseCursorX = destX;
            StartEraseCursorStartX = null;
            StartEraseCursorDestX = null;
        }
    }

    protected override void Init()
    {
        // Reset values
        SelectedStartEraseOption = 0;
        EraseSaveStage = StartEraseMode.Selection;
        StartEraseCursorX = 0;
        StartEraseCursorStartX = null;
        StartEraseCursorDestX = null;

        // Add slots
        foreach (ModernMenuAll.Slot slot in Menu.Slots)
            AddOption(new SlotMenuOption(slot));

        // Create animations
        AnimatedObjectResource startEraseAnimations = Rom.Loader.ReadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuStartEraseAnimations);

        StartEraseSelection = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(110, 30),
            CurrentAnimation = Rayman3.Loc.LanguageUiIndex * 2 + 1,
            RenderContext = RenderContext,
        };

        ReadvancedStartEraseSelection = new SpriteTextureObject()
        {
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(74, -13),
            RenderContext = RenderContext,
        };
        SetReadvancedStartEraseSelectionTexture(0);

        StartEraseCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(CursorBaseX + StartEraseCursorX, 12),
            CurrentAnimation = 40,
            RenderContext = RenderContext,
        };
    }

    protected override void Step_TransitionIn()
    {
        ReadvancedStartEraseSelection.ScreenPos = ReadvancedStartEraseSelection.ScreenPos with { Y = TransitionValue / 2f - 93 };
        StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = TransitionValue / 2f - 68 };
    }

    protected override void Step_Active()
    {
        switch (EraseSaveStage)
        {
            case StartEraseMode.Selection:
                // Move left
                if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeftExt))
                {
                    int newIndex = SelectedStartEraseOption - 1;
                    if (newIndex < 0)
                        newIndex = 2;

                    SelectStartEraseOption(newIndex);
                    SetReadvancedStartEraseSelectionTexture(newIndex);
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                // Move right
                else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuRightExt))
                {
                    int newIndex = SelectedStartEraseOption + 1;
                    if (newIndex > 2)
                        newIndex = 0;

                    SelectStartEraseOption(newIndex);
                    SetReadvancedStartEraseSelectionTexture(newIndex);
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                // Move up
                else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
                {
                    SetSelectedOption(SelectedOption - 1);
                }
                // Move down
                else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
                {
                    SetSelectedOption(SelectedOption + 1);
                }
                // Select slot
                else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                {
                    switch (SelectedStartEraseOption)
                    {
                        // Start game
                        case 0:
                            // New game
                            if (Menu.Slots[SelectedOption] == null)
                            {
                                Menu.ChangePage(new NewGameMenuPage(Menu, SelectedOption), NewPageMode.Next);
                            }
                            // Existing game
                            else
                            {
                                CursorClick(() =>
                                {
                                    Engine.Sem.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                                    FadeOut(2, () =>
                                    {
                                        Engine.Sem.StopAllSongs();

                                        // Load an existing game
                                        Rayman3.GameInfo.Load(SelectedOption);
                                        Rayman3.GameInfo.GotoLastSaveGame();

                                        Rayman3.GameInfo.StartPlayTime();

                                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                                        Gfx.Fade = AlphaCoefficient.Max;

                                        Rayman3.GameInfo.CurrentSlot = SelectedOption;
                                    });
                                });
                            }
                            break;

                        // Erase game
                        case 1:
                            if (Menu.Slots[SelectedOption] != null)
                            {
                                EraseSaveStage = StartEraseMode.TransitionOutSelection;
                                TransitionValue = 0;
                                CursorClick(null);
                            }
                            else
                            {
                                InvalidCursorClick();
                            }
                            break;

                        // Export game
                        case 2:
                            if (Menu.Slots[SelectedOption] != null)
                            {
                                Menu.ChangePage(new ExportSaveMenuPage(Menu, SelectedOption), NewPageMode.Next);
                            }
                            else
                            {
                                InvalidCursorClick();
                            }
                            break;
                    }
                }
                break;

            case StartEraseMode.TransitionOutSelection:
                TransitionValue += 4;
                ReadvancedStartEraseSelection.ScreenPos = ReadvancedStartEraseSelection.ScreenPos with { Y = -13 - TransitionValue };
                StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    EraseSaveStage = StartEraseMode.TransitionInConfirmErase;
                    StartEraseSelection.CurrentAnimation = Rayman3.Loc.LanguageUiIndex * 2 + 21;
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
                if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeftExt))
                {
                    if (SelectedStartEraseOption != 0)
                    {
                        SelectStartEraseOption(0);
                        StartEraseSelection.CurrentAnimation = Rayman3.Loc.LanguageUiIndex * 2 + 20;
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Move right
                else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuRightExt))
                {
                    if (SelectedStartEraseOption != 1)
                    {
                        SelectStartEraseOption(1);
                        StartEraseSelection.CurrentAnimation = Rayman3.Loc.LanguageUiIndex * 2 + 21;
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }
                // Erase slot
                else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                {
                    EraseSaveStage = StartEraseMode.TransitionOutConfirmErase;
                    TransitionValue = 0;
                    if (SelectedStartEraseOption == 0 && Menu.Slots[SelectedOption] != null)
                    {
                        Menu.Slots[SelectedOption] = null;
                        ((SlotMenuOption)Options[SelectedOption]).Slot = null;
                        Rayman3.Save.DeleteSlot(SelectedOption);
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
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
                    SetReadvancedStartEraseSelectionTexture(SelectedStartEraseOption);
                    StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = -68 };
                }
                break;

            case StartEraseMode.TransitionInSelection:
                TransitionValue += 4;
                ReadvancedStartEraseSelection.ScreenPos = ReadvancedStartEraseSelection.ScreenPos with { Y = TransitionValue - 77 };
                StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = TransitionValue - 52 };

                if (TransitionValue >= 64)
                {
                    TransitionValue = 0;
                    EraseSaveStage = StartEraseMode.Selection;
                }
                break;
        }

        if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack) && !TransitionsFX.IsFadingOut)
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

        ManageStartEraseCursor();
    }

    protected override void Step_TransitionOut()
    {
        ReadvancedStartEraseSelection.ScreenPos = ReadvancedStartEraseSelection.ScreenPos with { Y = -13 - TransitionValue / 2f };
        StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { Y = 12 - TransitionValue / 2f };
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
        
        if (EraseSaveStage is 
            StartEraseMode.TransitionInConfirmErase or 
            StartEraseMode.ConfirmErase or 
            StartEraseMode.TransitionOutConfirmErase)
            animationPlayer.Play(StartEraseSelection);
        
        animationPlayer.Play(ReadvancedStartEraseSelection);

        StartEraseCursor.ScreenPos = StartEraseCursor.ScreenPos with { X = CursorBaseX + StartEraseCursorX };
        animationPlayer.Play(StartEraseCursor);
    }
}