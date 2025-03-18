using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class PauseDialog : Dialog
{
    public PauseDialog(Scene2D scene) : base(scene)
    {
        PausedMachineId = 0xff;
    }

    public AnimatedObject Canvas { get; set; }
    public AnimatedObject Cursor { get; set; }
    public AnimatedObject PauseSelection { get; set; }
    public SpriteTextObject[] SleepModeTexts { get; set; }

    // N-Gage exclusive
    public AnimatedObject SelectSymbol { get; set; }
    public AnimatedObject BackSymbol { get; set; }
    public AnimatedObject MusicVolume { get; set; }
    public AnimatedObject SfxVolume { get; set; }

    // Custom properties to simulate sleep mode
    public bool IsInSleepMode { get; set; }
    public int SleepModeTimer { get; set; }
    public FadeControl SavedFadeControl { get; set; }
    public float SavedFade { get; set; }

    public int PausedMachineId { get; set; }

    public int SelectedOption { get; set; }
    public int PrevSelectedOption { get; set; }

    public int OffsetY { get; set; }
    public int CursorOffsetY { get; set; }
    public PauseDialogDrawStep DrawStep { get; set; }

    private void ManageCursor()
    {
        if (SelectedOption != PrevSelectedOption)
        {
            const int lineHeight = 16;
            int targetY = SelectedOption * lineHeight;

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

        Cursor.ScreenPos = Cursor.ScreenPos with { Y = CursorOffsetY + 88 - OffsetY };
    }

    private void SetMusicVolumeAnimation()
    {
        switch (((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume)
        {
            case 0:
                MusicVolume.CurrentAnimation = 30;
                break;

            case 0x20:
                MusicVolume.CurrentAnimation = 31;
                break;

            case 0x40:
                MusicVolume.CurrentAnimation = 32;
                break;

            case 0x80:
                MusicVolume.CurrentAnimation = 33;
                break;
        }
    }

    private void SetSfxVolumeAnimation()
    {
        switch (((NGageSoundEventsManager)SoundEventsManager.Current).SfxVolume)
        {
            case 0:
                SfxVolume.CurrentAnimation = 30;
                break;

            case 0x20:
                SfxVolume.CurrentAnimation = 31;
                break;

            case 0x40:
                SfxVolume.CurrentAnimation = 32;
                break;

            case 0x80:
                SfxVolume.CurrentAnimation = 33;
                break;
        }
    }

    private void ModifyMusicVolume(int volDelta)
    {
        float currentVolume = ((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume;
        float newVolume = 0;
        
        if (0 < volDelta)
        {
            newVolume = (int)currentVolume << volDelta;
            
            if (newVolume == 0)
                newVolume = 0x20;
            
            if (newVolume >= SoundEngineInterface.MaxVolume)
                newVolume = SoundEngineInterface.MaxVolume;
        }
        else if (volDelta < 0)
        {
            newVolume = (int)currentVolume >> -volDelta;

            if (newVolume < 0x20)
                newVolume = 0;
        }

        ((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume = newVolume;
    }

    private void ModifySfxVolume(int volDelta)
    {
        float currentVolume = ((NGageSoundEventsManager)SoundEventsManager.Current).SfxVolume;
        float newVolume = 0;

        if (0 < volDelta)
        {
            newVolume = (int)currentVolume << volDelta;

            if (newVolume == 0)
                newVolume = 0x20;

            if (newVolume >= SoundEngineInterface.MaxVolume)
                newVolume = SoundEngineInterface.MaxVolume;
        }
        else if (volDelta < 0)
        {
            newVolume = (int)currentVolume >> -volDelta;

            if (newVolume < 0x20)
                newVolume = 0;
        }

        ((NGageSoundEventsManager)SoundEventsManager.Current).SfxVolume = newVolume;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public override void Load()
    {
        // NOTE: Game has it set up so Load can be called multiple times. Dynamic objects don't get recreated after the first time, but instead
        //       reloaded into VRAM. We don't need to do that though due to how the graphics system works here, so just always create everything.

        if (Rom.Platform == Platform.NGage)
            ((NGageSoundEventsManager)SoundEventsManager.Current).PauseLoopingSoundEffects();

        AnimatedObjectResource canvasResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.PauseCanvasAnimations);
        Canvas = new AnimatedObject(canvasResource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(Rom.Platform switch
            {
                Platform.GBA => 106,
                Platform.NGage => 76,
                _ => throw new UnsupportedPlatformException()
            }, 80),
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
            }, 88),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        CursorOffsetY = 0;

        AnimatedObjectResource selectionsResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.PauseSelectionAnimations);
        PauseSelection = new AnimatedObject(selectionsResource, true)
        {
            IsFramed = true,
            CurrentAnimation = RSMultiplayer.IsActive && Rom.Platform == Platform.GBA 
                ? Localization.LanguageUiIndex + 50 
                : Localization.LanguageUiIndex,
            ScreenPos = new Vector2(Rom.Platform switch
            {
                Platform.GBA => 84,
                Platform.NGage => 54,
                _ => throw new UnsupportedPlatformException()
            }, 90),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext
        };

        if (Rom.Platform == Platform.GBA)
        {
            string[] textLines = Localization.GetText(TextBankId.Connectivity, 15);

            SleepModeTexts = new SpriteTextObject[4];
            for (int i = 0; i < SleepModeTexts.Length; i++)
            {
                SleepModeTexts[i] = new SpriteTextObject()
                {
                    Text = i < textLines.Length ? textLines[i] : "",
                    BgPriority = 0,
                    ObjPriority = 0,
                    Color = TextColor.SleepMode,
                    FontSize = FontSize.Font16,
                    RenderContext = Rom.OriginalScaledGameRenderContext,
                };

                SleepModeTexts[i].ScreenPos = new Vector2(Rom.OriginalScaledGameRenderContext.Resolution.X / 2 - SleepModeTexts[i].GetStringWidth() / 2f, i * 16 + 50);
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            AnimatedObjectResource symbolsResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.NGageButtonSymbols);
            SelectSymbol = new AnimatedObject(symbolsResource, false)
            {
                IsFramed = true,
                CurrentAnimation = Localization.LanguageUiIndex,
                ScreenPos = new Vector2(-1, -18),
                VerticalAnchor = VerticalAnchorMode.Bottom,
                RenderContext = Scene.HudRenderContext,
            };
            BackSymbol = new AnimatedObject(symbolsResource, false)
            {
                IsFramed = true,
                CurrentAnimation = 5 + Localization.LanguageUiIndex,
                ScreenPos = new Vector2(-1, -18), // Set X when drawing
                HorizontalAnchor = HorizontalAnchorMode.Right,
                VerticalAnchor = VerticalAnchorMode.Bottom,
                RenderContext = Scene.HudRenderContext,
            };

            MusicVolume = new AnimatedObject(selectionsResource, true)
            {
                IsFramed = true,
                CurrentAnimation = 30,
                ScreenPos = new Vector2(130, 91),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
            SfxVolume = new AnimatedObject(selectionsResource, true)
            {
                IsFramed = true,
                CurrentAnimation = 30,
                ScreenPos = new Vector2(130, 109),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };

            SetMusicVolumeAnimation();
            SetSfxVolumeAnimation();
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
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
        // The original game does this in the state machine, but since we're still running
        // the game loop while in the simulated sleep mode we have to do it here
        if (IsInSleepMode)
        {
            foreach (SpriteTextObject text in SleepModeTexts)
                animationPlayer.Play(text);
            return;
        }

        switch (DrawStep)
        {
            case PauseDialogDrawStep.Hide:
                OffsetY = 130;
                break;
            
            case PauseDialogDrawStep.MoveIn:
                if (OffsetY < 1)
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
            Canvas.FrameChannelSprite();

            Canvas.ScreenPos = Canvas.ScreenPos with { Y = 80 - OffsetY };
            ManageCursor();
            PauseSelection.ScreenPos = PauseSelection.ScreenPos with { Y = 90 - OffsetY };

            if (Rom.Platform == Platform.NGage)
            {
                MusicVolume.ScreenPos = MusicVolume.ScreenPos with { Y = 91 - OffsetY };
                SfxVolume.ScreenPos = SfxVolume.ScreenPos with { Y = 109 - OffsetY };
            }

            animationPlayer.Play(Canvas);
            animationPlayer.Play(Cursor);
            animationPlayer.Play(PauseSelection);

            if (Rom.Platform == Platform.NGage)
            {
                animationPlayer.Play(MusicVolume);
                animationPlayer.Play(SfxVolume);

                SelectSymbol.CurrentAnimation = Localization.LanguageUiIndex;
                BackSymbol.CurrentAnimation = 5 + Localization.LanguageUiIndex;

                BackSymbol.ScreenPos = BackSymbol.ScreenPos with
                {
                    X = Localization.LanguageUiIndex switch
                    {
                        1 => -55,
                        2 => -50,
                        3 => -53,
                        4 => -62,
                        _ => -41,
                    }
                };

                animationPlayer.PlayFront(SelectSymbol);
                animationPlayer.PlayFront(BackSymbol);
            }
        }
    }
}