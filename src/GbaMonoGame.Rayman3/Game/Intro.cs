using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

public class Intro : Frame, IHasPlayfield
{
    #region Constant Fields

    // NOTE: The game uses 16, but it only updates every 2 frames. We instead update every frame.
    private const int PaletteFadeMaxValue = 16 * 2;

    #endregion

    #region Properties

    public AnimationPlayer AnimationPlayer { get; set; }
    public TgxPlayfield2D Playfield { get; set; }

    public Action CurrentStepAction { get; set; }
    public MenuAll Menu { get; set; }
    public AnimatedObject PressStartObj { get; set; }
    public AnimatedObject GameloftLogoObj { get; set; }
    public AnimatedObject BlackLumAndLogoObj { get; set; }
    public int Timer { get; set; }
    public int AlphaTimer { get; set; }
    public int ScrollY { get; set; }
    public bool IsSkipping { get; set; }
    public int PaletteFadeValue { get; set; }
    public int SkippedTimer { get; set; }

    #endregion

    #region Interface Properties

    TgxPlayfield IHasPlayfield.Playfield => Playfield;

    #endregion

    #region Private Methods

    private void Skip()
    {
        PaletteFadeValue--;

        float colorValue = PaletteFadeValue / (float)PaletteFadeMaxValue;
        Gfx.Color = new Color(colorValue, colorValue, colorValue, 1);

        if (PaletteFadeValue == 0)
        {
            CurrentStepAction = Step_Skip_1;
            SkippedTimer = 0;
        }
    }

    #endregion

    #region Pubic Override Methods

    public override void Init()
    {
        SoundEngineInterface.SetNbVoices(10);

        // Pre-load the menu
        Menu = new MenuAll(Rom.Platform switch
        {
            Platform.GBA => MenuAll.Page.SelectLanguage,
            Platform.NGage => MenuAll.Page.NGage_FirstPage,
            _ => throw new UnsupportedPlatformException(),
        });
        Menu.LoadGameInfo();

        AnimationPlayer = new AnimationPlayer(true, SoundEventsManager.ProcessEvent);

        AnimatedObjectResource introAnimResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.IntroAnimations);

        PressStartObj = new AnimatedObject(introAnimResource, false)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(120, 150),
                Platform.NGage => new Vector2(88, 150),
                _ => throw new UnsupportedPlatformException(),
            },
            CurrentAnimation = Rom.Platform switch
            {
                Platform.GBA => 9,
                Platform.NGage => 9 + Localization.LanguageUiIndex,
                _ => throw new UnsupportedPlatformException(),
            },
            RenderContext = Rom.OriginalGameRenderContext,
        };

        if (Rom.Platform == Platform.NGage)
        {
            GameloftLogoObj = new AnimatedObject(introAnimResource, false)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(88, 208),
                CurrentAnimation = 23,
                RenderContext = Rom.OriginalGameRenderContext,
            };
        }

        BlackLumAndLogoObj = new AnimatedObject(introAnimResource, false)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(120, 128),
                Platform.NGage => new Vector2(88, 128),
                _ => throw new UnsupportedPlatformException(),
            },
            CurrentAnimation = 0,
            RenderContext = Rom.OriginalGameRenderContext,
        };

        PlayfieldResource introPlayfield = Rom.LoadResource<PlayfieldResource>(GameResource.IntroPlayfield);
        Playfield = TgxPlayfield.Load<TgxPlayfield2D>(introPlayfield);
        Playfield.RenderContext.SetFixedResolution(Rom.OriginalResolution);

        Gfx.ClearColor = Color.Black;

        Playfield.Camera.Position = Vector2.Zero;
        Playfield.Step();

        if (Rom.Platform == Platform.GBA)
        {
            Playfield.TileLayers[0].Screen.IsEnabled = false;
            Playfield.TileLayers[1].Screen.IsEnabled = false;
            Playfield.TileLayers[3].Screen.IsEnabled = false;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Playfield.TileLayers[0].Screen.IsEnabled = true;
            Playfield.TileLayers[1].Screen.IsEnabled = true;
            Playfield.TileLayers[2].Screen.IsEnabled = false;
            Playfield.TileLayers[3].Screen.IsEnabled = false;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        // TODO: Allow scrolling on N-Gage too?
        if (Rom.Platform == Platform.GBA)
        {
            TextureScreenRenderer renderer = ((TextureScreenRenderer)Playfield.TileLayers[3].Screen.Renderer);
            Playfield.TileLayers[3].Screen.Renderer = new IntroCloudsRenderer(renderer.Texture);
        }

        Gfx.FadeControl = FadeControl.None;

        CurrentStepAction = Step_1;

        AlphaTimer = 0;
        Timer = 0;
        ScrollY = 0;
        IsSkipping = false;
        PaletteFadeValue = PaletteFadeMaxValue;

        SoundEventsManager.SetVolumeForType(SoundType.Music, 0);
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__sadslide);
    }

    public override void UnInit()
    {
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__sadslide);

        Playfield.UnInit();

        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        SoundEventsManager.SetVolumeForType(SoundType.Music, SoundEngineInterface.MaxVolume);
    }

    public override void Step()
    {
        Playfield.Step();
        AnimationPlayer.Execute();

        // Fade in music
        if (GameTime.ElapsedFrames <= 64)
            SoundEventsManager.SetVolumeForType(SoundType.Music, GameTime.ElapsedFrames * 2);

        CurrentStepAction();

        // TODO: Only allow this if in debug mode
        // NOTE: This cheat is not included in the retail versions of the games
        if (JoyPad.IsButtonJustPressed(GbaInput.L))
        {
            FrameManager.SetNextFrame(new LevelSelect());
            Localization.SetLanguage(0);
            Random.SetSeed(GameTime.ElapsedFrames);
        }
    }

    #endregion

    #region Steps

    private void Step_1()
    {
        if (Rom.Platform == Platform.GBA)
        {
            Timer++;

            if (Timer > 60)
            {
                Timer = 0;
                CurrentStepAction = Step_2;

                Playfield.TileLayers[0].Screen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                Playfield.TileLayers[0].Screen.GbaAlpha = 0;

                Playfield.TileLayers[0].Screen.IsEnabled = true;
                Playfield.TileLayers[1].Screen.IsEnabled = true;
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            CurrentStepAction = Step_2;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private void Step_2()
    {
        if (Rom.Platform == Platform.GBA)
        {
            Timer++;

            if (Timer >= 68)
            {
                Timer = 0;
                CurrentStepAction = Step_3;

                Playfield.TileLayers[0].Screen.RenderOptions.BlendMode = BlendMode.None;
                Playfield.TileLayers[2].Screen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                Playfield.TileLayers[2].Screen.GbaAlpha = 16;
            }
            else
            {
                Playfield.TileLayers[0].Screen.GbaAlpha = Timer / 4f;
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            CurrentStepAction = Step_3;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private void Step_3()
    {
        Timer++;

        if (Timer >= 68)
        {
            Timer = 0;
            AlphaTimer = 0;
            CurrentStepAction = Step_4;

            Playfield.TileLayers[2].Screen.RenderOptions.BlendMode = BlendMode.None;
            Playfield.TileLayers[3].Screen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
            Playfield.TileLayers[3].Screen.GbaAlpha = 0;

            Playfield.TileLayers[2].Screen.IsEnabled = false;

            // Only show clouds on GBA
            if (Rom.Platform == Platform.GBA)
                Playfield.TileLayers[3].Screen.IsEnabled = true;
        }
        else
        {
            Playfield.TileLayers[2].Screen.GbaAlpha = 16 - (Timer / 4f);
        }

        // N-Gage allows the intro to be skipped from here
        if (Rom.Platform == Platform.NGage)
        {
            // TODO: Check every button input
            if (JoyPad.IsButtonPressed(GbaInput.Start))
                IsSkipping = true;

            if (IsSkipping)
                Skip();
        }
    }

    private void Step_4()
    {
        // Get the main cluster
        TgxCluster mainCluster = Playfield.Camera.GetMainCluster();

        // Scroll if we haven't yet reached the bottom
        if (!mainCluster.IsOnLimit(Edge.Bottom))
        {
            ScrollY++;
            Playfield.Camera.Position += new Vector2(0, 1);
            Gfx.GetScreen(3).Offset = new Vector2(0, ScrollY);
        }

        if (ScrollY > 600 && AlphaTimer <= 0x80)
        {
            Playfield.TileLayers[3].Screen.GbaAlpha = AlphaTimer / 32f;
            AlphaTimer++;
        }

        if (ScrollY > 175)
        {
            if (BlackLumAndLogoObj.EndOfAnimation)
            {
                if (BlackLumAndLogoObj.CurrentAnimation < 4)
                {
                    BlackLumAndLogoObj.CurrentAnimation++;
                    BlackLumAndLogoObj.ScreenPos = Rom.Platform switch
                    {
                        Platform.GBA => new Vector2(120, 128),
                        Platform.NGage => new Vector2(88, 128),
                        _ => throw new UnsupportedPlatformException(),
                    };
                }
                else
                {
                    BlackLumAndLogoObj.CurrentAnimation = 6;
                    BlackLumAndLogoObj.ScreenPos = Rom.Platform switch
                    {
                        Platform.GBA => new Vector2(120, 70),
                        Platform.NGage => new Vector2(88, 70),
                        _ => throw new UnsupportedPlatformException(),
                    };
                }
            }

            if (BlackLumAndLogoObj.CurrentAnimation < 5)
            {
                BlackLumAndLogoObj.ScreenPos -= new Vector2(0, 1); // NOTE: Game moves by 2 every second frame
                Timer++;

                BlackLumAndLogoObj.FrameChannelSprite();
                AnimationPlayer.PlayFront(BlackLumAndLogoObj);
            }
        }

        if (Rom.Platform == Platform.GBA)
        {
            if (JoyPad.IsButtonPressed(GbaInput.Start) && ScrollY <= 863)
                IsSkipping = true;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            // TODO: Check every button input
            if (JoyPad.IsButtonPressed(GbaInput.Start))
                IsSkipping = true;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        if (IsSkipping)
            Skip();
        else if (mainCluster.IsOnLimit(Edge.Bottom))
            CurrentStepAction = Step_5;
    }

    private void Step_5()
    {
        if (BlackLumAndLogoObj.EndOfAnimation)
        {
            if (BlackLumAndLogoObj.CurrentAnimation == 7)
            {
                BlackLumAndLogoObj.CurrentAnimation = 8;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__raytheme__After__sadslide);
                CurrentStepAction = Step_6;
            }
            else
            {
                BlackLumAndLogoObj.CurrentAnimation++;
            }
        }

        BlackLumAndLogoObj.FrameChannelSprite();
        AnimationPlayer.PlayFront(BlackLumAndLogoObj);
    }

    private void Step_6()
    {
        if ((GameTime.ElapsedFrames & 0x10) != 0)
            AnimationPlayer.PlayFront(PressStartObj);

        if (Rom.Platform == Platform.NGage)
            AnimationPlayer.PlayFront(GameloftLogoObj);

        BlackLumAndLogoObj.FrameChannelSprite();
        AnimationPlayer.PlayFront(BlackLumAndLogoObj);

        // TODO: Check every button input on N-Gage
        if (JoyPad.IsButtonPressed(GbaInput.Start))
        {
            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
            Gfx.Fade = 1;

            FrameManager.SetNextFrame(Menu);
            Random.SetSeed(GameTime.ElapsedFrames);
        }
    }

    private void Step_Skip_1()
    {
        SkippedTimer++;

        if (SkippedTimer == 10)
        {
            AlphaTimer = 0x80;
            Playfield.TileLayers[3].Screen.GbaAlpha = AlphaTimer / 32f;
            BlackLumAndLogoObj.CurrentAnimation = 8;
            BlackLumAndLogoObj.CurrentFrame = 5;
            BlackLumAndLogoObj.ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(120, 70),
                Platform.NGage => new Vector2(88, 70),
                _ => throw new UnsupportedPlatformException(),
            };

            Playfield.Camera.Position = new Vector2(0, 880);
            ScrollY = 879;
        }
        else if (SkippedTimer == 20)
        {
            CurrentStepAction = Step_Skip_2;
        }
    }

    private void Step_Skip_2()
    {
        PaletteFadeValue++;

        if (PaletteFadeValue <= PaletteFadeMaxValue)
        {
            float colorValue = PaletteFadeValue / (float)PaletteFadeMaxValue;
            Gfx.Color = new Color(colorValue, colorValue, colorValue, 1);
        }

        if (PaletteFadeValue == PaletteFadeMaxValue - 2)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__raytheme__After__sadslide);
        }
        else if (PaletteFadeValue > PaletteFadeMaxValue)
        {
            CurrentStepAction = Step_6;
        }

        if ((GameTime.ElapsedFrames & 0x10) != 0)
            AnimationPlayer.PlayFront(PressStartObj);

        if (Rom.Platform == Platform.NGage)
            AnimationPlayer.PlayFront(GameloftLogoObj);

        BlackLumAndLogoObj.FrameChannelSprite();
        AnimationPlayer.PlayFront(BlackLumAndLogoObj);
    }

    #endregion
}