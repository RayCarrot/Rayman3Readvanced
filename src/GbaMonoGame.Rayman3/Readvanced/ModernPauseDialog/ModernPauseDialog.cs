using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class ModernPauseDialog : Dialog
{
    public ModernPauseDialog(Scene2D scene, bool canExitLevel) : base(scene)
    {
        CanExitLevel = canExitLevel;
        PausedMachineId = -1;
    }

    private const int LineHeight = 16;
    private const float CursorBaseY = 88;
    private const float CanvasBaseY = 80;
    private const float OptionsBaseY = 94;

    public AnimatedObject Canvas { get; set; }
    public AnimatedObject Cursor { get; set; }
    public SpriteFontTextObject[] Options { get; set; }

    public PauseDialogOptionsMenu OptionsMenu { get; set; }

    public int SelectedOption { get; set; }
    public int PrevSelectedOption { get; set; }
    public int OptionsCount { get; set; }

    public int OffsetY { get; set; }
    public int CursorOffsetY { get; set; }
    public PauseDialogDrawStep DrawStep { get; set; }

    public bool CanExitLevel { get; }
    public int PausedMachineId { get; set; }

    public CircleTransitionScreenEffect CircleTransitionScreenEffect { get; set; }
    public int CircleTransitionValue { get; set; }

    private void ManageCursor()
    {
        if (SelectedOption != PrevSelectedOption)
        {
            int targetY = SelectedOption * LineHeight;

            if (SelectedOption < PrevSelectedOption)
            {
                if (targetY < CursorOffsetY)
                {
                    CursorOffsetY -= 2;
                }
                else
                {
                    CursorOffsetY = targetY;
                    PrevSelectedOption = SelectedOption;
                }
            }
            else
            {
                if (targetY > CursorOffsetY)
                {
                    CursorOffsetY += 2;
                }
                else
                {
                    CursorOffsetY = targetY;
                    PrevSelectedOption = SelectedOption;
                }
            }
        }

        Cursor.ScreenPos = Cursor.ScreenPos with { Y = CursorOffsetY + CursorBaseY - OffsetY };
    }

    private void SetOptions(string[] options)
    {
        for (int i = 0; i < Options.Length; i++)
            Options[i].Text = i < options.Length ? options[i] : null;

        OptionsCount = options.Length;
    }

    private void SetSelectedOption(int selectedOption)
    {
        if (selectedOption < 0)
            selectedOption = OptionsCount - 1;
        else if (selectedOption > OptionsCount - 1)
            selectedOption = 0;

        PrevSelectedOption = SelectedOption;
        SelectedOption = selectedOption;

        for (int i = 0; i < Options.Length; i++)
            Options[i].Font = i == SelectedOption ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public override void Load()
    {
        if (Rom.Platform == Platform.NGage)
            ((NGageSoundEventsManager)SoundEventsManager.Current).PauseLoopingSoundEffects();

        AnimatedObjectResource canvasResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.PauseCanvasAnimations);
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

        Options = new SpriteFontTextObject[3];
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

        OptionsMenu = new PauseDialogOptionsMenu();
        OptionsMenu.Load();

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
            State.SetTo(Fsm_CheckSelection);
        }

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
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
        }
    }
}