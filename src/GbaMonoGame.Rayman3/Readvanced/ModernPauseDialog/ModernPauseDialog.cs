using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using Action = System.Action;

namespace GbaMonoGame.Rayman3.Readvanced;

[GenerateFsmFields]
public partial class ModernPauseDialog : Dialog
{
    public ModernPauseDialog(Scene2D scene, bool canExitLevel) : base(scene)
    {
        CreateGeneratedStates();

        CanExitLevel = canExitLevel;
        PausedMachineId = -1;
    }

    private const int MaxOptions = 4;
    private const int LineHeight = 16;
    private const float CursorBaseY = 88;
    private const float CanvasBaseY = 80;
    private const float OptionsBaseY = 94;

    public AnimatedObject Canvas { get; set; }
    public AnimatedObject Cursor { get; set; }
    public SpriteFontTextObject[] Options { get; set; }
    public FiniteStateMachine.Fsm[] OptionStates { get; set; }
    public Action[] OptionActions { get; set; }

    public PauseDialogOptionsMenu OptionsMenu { get; set; }
    public PauseDialogLevelsMenu LevelsMenu { get; set; }

    public int SelectedOption { get; set; }
    public int SavedSelectedOption { get; set; }
    public int OptionsCount { get; set; }

    public float? CursorStartY { get; set; }
    public float? CursorDestY { get; set; }

    public int OffsetY { get; set; }
    public float CursorOffsetY { get; set; }
    public PauseDialogDrawStep DrawStep { get; set; }

    public bool CanExitLevel { get; }
    public int PausedMachineId { get; set; }

    public CircleTransitionScreenEffect CircleTransitionScreenEffect { get; set; }
    public int CircleTransitionValue { get; set; }
    public bool IsTransitioningOut => CircleTransitionScreenEffect != null;

    private void ManageCursor()
    {
        // Move with a constant speed of 2
        const float speed = 2;

        if (CursorStartY != null && CursorDestY != null)
        {
            float startY = CursorStartY.Value;
            float destY = CursorDestY.Value;

            // Move up
            if (destY < startY && CursorOffsetY > destY)
            {
                CursorOffsetY -= speed;
            }
            // Move down
            else if (destY > startY && CursorOffsetY < destY)
            {
                CursorOffsetY += speed;
            }
            // Finished moving
            else
            {
                CursorOffsetY = destY;
                CursorStartY = null;
                CursorDestY = null;
            }
        }

        Cursor.ScreenPos = Cursor.ScreenPos with { Y = CursorOffsetY + CursorBaseY - OffsetY };
    }

    private void ClearOptions()
    {
        OptionsCount = 0;

        foreach (SpriteFontTextObject option in Options)
            option.Text = null;

        Array.Clear(OptionStates);
        Array.Clear(OptionActions);
    }

    private void AddOption(string text, FiniteStateMachine.Fsm state, Action action = null)
    {
        int index = OptionsCount;
        OptionsCount++;

        if (index < MaxOptions)
        {
            Options[index].Text = text;
            OptionStates[index] = state;
            OptionActions[index] = action;
        }
    }

    private void MoveToSelectedOptionState()
    {
        State.MoveTo(OptionStates[SelectedOption]);
    }

    private void InvokeSelectedOption()
    {
        OptionActions[SelectedOption]?.Invoke();
    }

    private void SetSelectedOption(int selectedOption)
    {
        if (selectedOption < 0)
            selectedOption = OptionsCount - 1;
        else if (selectedOption > OptionsCount - 1)
            selectedOption = 0;

        SelectedOption = selectedOption;

        CursorStartY = CursorOffsetY;
        CursorDestY = selectedOption * LineHeight;

        for (int i = 0; i < Options.Length; i++)
            Options[i].Font = i == SelectedOption ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    private void BeginCircleTransition()
    {
        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__SlideOut_Mix01);

        CircleTransitionValue = 252;

        // Create the circle transition
        CircleTransitionScreenEffect = new CircleTransitionScreenEffect()
        {
            RenderContext = Scene.RenderContext,
        };

        // Initialize and add as a screen effect
        CircleTransitionScreenEffect.Init(CircleTransitionValue, Scene.RenderContext.Resolution / 2);
        Gfx.SetScreenEffect(CircleTransitionScreenEffect);
    }

    private bool StepCircleTransition()
    {
        CircleTransitionValue -= 6;

        if (CircleTransitionValue < 0)
        {
            CircleTransitionValue = 0;
            CircleTransitionScreenEffect = null;

            return true;
        }
        else
        {
            CircleTransitionScreenEffect.Radius = CircleTransitionValue;
            return false;
        }
    }

    private void MoveInMenu()
    {
        // Move out the user info bars
        switch (Frame.Current)
        {
            case FrameSideScroller frameSideScroller:
                // Move out the life, lums and cages bars
                frameSideScroller.UserInfo.MoveOutBars();

                // The Lums1000Bar can't normally transition out, so we force it to here
                frameSideScroller.UserInfo.Lums1000Bar?.EnableTransitions = true;

                frameSideScroller.TimeAttackDialog?.MoveOutBars();
                break;

            case FrameSingleMode7 frameSingleMode7:
                frameSingleMode7.UserInfo.MoveOutBars();

                frameSingleMode7.TimeAttackDialog?.MoveOutBars();
                break;

            case FrameWaterSkiMode7 frameWaterSkiMode7:
                frameWaterSkiMode7.UserInfo.MoveOutBars();
                break;

            case FrameWorldSideScroller frameWorldSideScroller:
                frameWorldSideScroller.UserInfo.LifeBar.MoveOut();

                frameWorldSideScroller.UserInfo.Lums1000Bar.MoveOut();
                frameWorldSideScroller.UserInfo.Lums1000Bar.EnableTransitions = true;

                frameWorldSideScroller.UserInfo.Cages50Bar.MoveOut();
                frameWorldSideScroller.UserInfo.Cages50Bar.EnableTransitions = true;

                frameWorldSideScroller.UserInfo.MoveOutCurtains();
                break;

            case WorldMap worldMap:
                worldMap.UserInfo.LifeBar.MoveOut();

                worldMap.UserInfo.Lums1000Bar.MoveOut();
                worldMap.UserInfo.Lums1000Bar.EnableTransitions = true;

                worldMap.UserInfo.Cages50Bar.MoveOut();
                worldMap.UserInfo.Cages50Bar.EnableTransitions = true;
                break;

            default:
                throw new InvalidOperationException("Invalid frame");
        }
    }

    private void MoveOutMenu()
    {
        // Move back in the user info bars
        switch (Frame.Current)
        {
            case FrameSideScroller frameSideScroller:
                // Only move in the life, lums and cages bars if we're not in a Ly challenge level
                if (GameInfo.MapId is not (MapId.ChallengeLy1 or MapId.ChallengeLy2 or MapId.ChallengeLyGCN))
                    frameSideScroller.UserInfo.MoveInBars();

                frameSideScroller.TimeAttackDialog?.MoveInBars();
                break;

            case FrameSingleMode7 frameSingleMode7:
                frameSingleMode7.UserInfo.MoveInBars();

                frameSingleMode7.TimeAttackDialog?.MoveInBars();
                break;

            case FrameWaterSkiMode7 frameWaterSkiMode7:
                frameWaterSkiMode7.UserInfo.MoveInBars();
                break;

            case FrameWorldSideScroller frameWorldSideScroller:
                frameWorldSideScroller.UserInfo.LifeBar.SetToStayVisible();
                frameWorldSideScroller.UserInfo.LifeBar.MoveIn();

                frameWorldSideScroller.UserInfo.Lums1000Bar.SetToStayVisible();
                frameWorldSideScroller.UserInfo.Lums1000Bar.MoveIn();

                frameWorldSideScroller.UserInfo.Cages50Bar.SetToStayVisible();
                frameWorldSideScroller.UserInfo.Cages50Bar.MoveIn();

                frameWorldSideScroller.UserInfo.MoveInCurtains();
                break;

            case WorldMap worldMap:
                worldMap.UserInfo.LifeBar.SetToStayVisible();
                worldMap.UserInfo.LifeBar.MoveIn();

                worldMap.UserInfo.Lums1000Bar.SetToStayVisible();
                worldMap.UserInfo.Lums1000Bar.MoveIn();

                worldMap.UserInfo.Cages50Bar.SetToStayVisible();
                worldMap.UserInfo.Cages50Bar.MoveIn();
                break;

            default:
                throw new InvalidOperationException("Invalid frame");
        }
    }

    public override void Load()
    {
        if (Rom.Platform == Platform.NGage)
            ((NGageSoundEventsManager)Engine.Sem).PauseLoopingSoundEffects();

        AnimatedObjectResource canvasResource = Rom.Loader.ReadResource<AnimatedObjectResource>(Rayman3DefinedResource.PauseCanvasAnimations);
        Canvas = new AnimatedObject(canvasResource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(Rom.Platform switch
            {
                Platform.GBA => 106,
                Platform.NGage => 76,
                _ => throw new UnsupportedPlatformException()
            }, CanvasBaseY),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };
        Cursor = new AnimatedObject(canvasResource, false)
        {
            IsFramed = true,
            CurrentAnimation = 1,
            ScreenPos = new Vector2(Rom.Platform switch
            {
                Platform.GBA => 66,
                Platform.NGage => 36,
                _ => throw new UnsupportedPlatformException()
            }, CursorBaseY),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        Options = new SpriteFontTextObject[MaxOptions];
        for (int i = 0; i < Options.Length; i++)
        {
            Options[i] = new SpriteFontTextObject
            {
                ScreenPos = new Vector2(Rom.Platform switch
                {
                    Platform.GBA => 85,
                    Platform.NGage => 55,
                    _ => throw new UnsupportedPlatformException()
                }, OptionsBaseY + LineHeight * i),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
        }

        OptionStates = new FiniteStateMachine.Fsm[MaxOptions];
        OptionActions = new Action[MaxOptions];

        OptionsMenu = new PauseDialogOptionsMenu();
        OptionsMenu.Load();

        if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
        {
            LevelsMenu = new PauseDialogLevelsMenu(Scene);
            LevelsMenu.Load();
        }

        CursorOffsetY = 0;
    }

    public override void Init()
    {
        SelectedOption = 0;
        OffsetY = 130;
        DrawStep = PauseDialogDrawStep.MoveIn;

        if (RSMultiplayer.IsActive)
        {
            // TODO: Implement Fsm_CheckSelectionMulti
        }
        else
        {
            State.SetTo(_Fsm_CheckSelection);
        }

        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        switch (DrawStep)
        {
            case PauseDialogDrawStep.Hide:
                OffsetY = 130;
                break;
            
            case PauseDialogDrawStep.MoveIn:
                if (OffsetY <= 0)
                {
                    DrawStep = PauseDialogDrawStep.Wait;
                    OffsetY = 0;
                }
                else
                {
                    OffsetY -= 4;
                }
                break;

            case PauseDialogDrawStep.MoveOut:
                if (OffsetY < 130)
                {
                    OffsetY += 4;
                }
                else
                {
                    OffsetY = 130;
                    DrawStep = PauseDialogDrawStep.Hide;
                }
                break;
        }

        if (DrawStep != PauseDialogDrawStep.Hide)
        {
            // Transition
            Canvas.ScreenPos = Canvas.ScreenPos with { Y = CanvasBaseY - OffsetY };
            ManageCursor();
            for (int i = 0; i < Options.Length; i++)
                Options[i].ScreenPos = Options[i].ScreenPos with { Y = OptionsBaseY + LineHeight * i - OffsetY };

            // Draw
            animationPlayer.Play(Canvas);
            animationPlayer.Play(Cursor);
            foreach (SpriteFontTextObject option in Options)
                animationPlayer.Play(option);
            OptionsMenu.Draw(animationPlayer);
            LevelsMenu?.Draw(animationPlayer);
        }
    }
}