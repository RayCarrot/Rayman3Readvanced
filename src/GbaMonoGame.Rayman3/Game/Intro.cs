using System;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

public class Intro : Frame, IHasPlayfield
{
    #region Constant Fields

    private const int SkyTileLayerId = 0;
    private const int MainTileLayerId = 1;
    private const int UbisoftLogoTileLayerId = 2;
    private const int CloudsTileLayerId = 3;

    // NOTE: The game uses 16, but it only updates every 2 frames. We instead update every frame.
    private const int PaletteFadeMaxValue = 16 * 2;

    #endregion

    #region Properties

    public AnimationPlayer AnimationPlayer { get; set; }
    public TgxPlayfield2D Playfield { get; set; }

    public Action CurrentStepAction { get; set; }
    public Frame Menu { get; set; }
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

    private void ReplaceLogo()
    {
        // Define the letters to replace
        var letters = new[]
        {
            // R
            new
            {
                ReplaceTile = 362,
                HideTiles = new[] { 426, 688 },
                OriginalOffset = new Point(-110, -46),
                NewOffset = new Point(2, 6),
                FileName = Assets.IntroLogo_Part1Texture
            },
            // A
            new
            {
                ReplaceTile = 430,
                HideTiles = new[] { 462 },
                OriginalOffset = new Point(-76, -27),
                NewOffset = new Point(33, 17),
                FileName = Assets.IntroLogo_Part2Texture
            },
            // Y
            new
            {
                ReplaceTile = 466,
                HideTiles = new[] { 482, 490 },
                OriginalOffset = new Point(-53, -28),
                NewOffset = new Point(56, 21),
                FileName = Assets.IntroLogo_Part3Texture
            },
            // M
            new
            {
                ReplaceTile = 492,
                HideTiles = Array.Empty<int>(),
                OriginalOffset = new Point(-25, -30),
                NewOffset = new Point(86, 22),
                FileName = Assets.IntroLogo_Part4Texture
            },
            // A
            new
            {
                ReplaceTile = 524,
                HideTiles = Array.Empty<int>(),
                OriginalOffset = new Point(3, -30),
                NewOffset = new Point(117, 21),
                FileName = Assets.IntroLogo_Part5Texture
            },
            // N
            new
            {
                ReplaceTile = 556,
                HideTiles = new[] { 588 },
                OriginalOffset = new Point(30, -27),
                NewOffset = new Point(142, 19),
                FileName = Assets.IntroLogo_Part6Texture
            },
            // ®
            new
            {
                ReplaceTile = -1,
                HideTiles = new[] { 360 },
                OriginalOffset = new Point(0, 0),
                NewOffset = new Point(0, 0),
                FileName = (string)null
            },
            // 3
            new
            {
                ReplaceTile = 596,
                HideTiles = new[] { 660, 676, 684 },
                OriginalOffset = new Point(62, -52),
                NewOffset = new Point(174, 5),
                FileName = Assets.IntroLogo_Part7Texture
            },
        };

        const int baseTileIndex = 1000;

        // Replace the sprite textures
        for (int letterIndex = 0; letterIndex < letters.Length; letterIndex++)
        {
            var letter = letters[letterIndex];
            if (letter.FileName != null)
                BlackLumAndLogoObj.ReplaceSpriteTexture(baseTileIndex + letterIndex, Engine.FrameContentManager.Load<Texture2D>(letter.FileName));
        }

        Point originalBaseOffset = letters[0].OriginalOffset;
        Point newBaseOffset = letters[0].NewOffset;

        // Replace the logo in animations 7 and 8
        replaceAnimation(7);
        replaceAnimation(8);

        void replaceAnimation(int animId)
        {
            // Ignore if the animation has already been replaced
            if (BlackLumAndLogoObj.HasReplacedAnimation(animId))
                return;

            // Create a copy of the animation
            Animation anim = BlackLumAndLogoObj.CopyAnimation(animId);

            // Enumerate every frame
            int channelIndex = 0;
            for (int frameIndex = 0; frameIndex < anim.FramesCount; frameIndex++)
            {
                // Enumerate every channel in the frame
                for (int frameChannelIndex = 0; frameChannelIndex < anim.ChannelsPerFrame[frameIndex]; frameChannelIndex++)
                {
                    AnimationChannel channel = anim.Channels[channelIndex];
                    if (channel.ChannelType == AnimationChannelType.Sprite)
                    {
                        for (int letterIndex = 0; letterIndex < letters.Length; letterIndex++)
                        {
                            var letter = letters[letterIndex];

                            // Replace the sprite
                            if (channel.TileIndex == letter.ReplaceTile)
                            {
                                Point offset = originalBaseOffset + (letter.NewOffset - newBaseOffset) - letter.OriginalOffset;

                                channel.TileIndex = (ushort)(baseTileIndex + letterIndex);
                                channel.XPosition += (short)offset.X;
                                channel.YPosition += (short)offset.Y;
                            }
                            // Hide the sprite
                            else if (Array.IndexOf(letter.HideTiles, channel.TileIndex) >= 0)
                            {
                                channel.ObjectMode = OBJ_ATTR_ObjectMode.HIDE;
                            }
                        }
                    }

                    channelIndex++;
                }
            }

            BlackLumAndLogoObj.ReplaceAnimation(animId, anim);
        }

        // Replace the "READVANCED" subtitle sprite texture
        BlackLumAndLogoObj.ReplaceSpriteTexture(baseTileIndex + letters.Length, Engine.FrameContentManager.Load<Texture2D>(Assets.IntroLogo_Part8Texture));

        Point newOffset = new(44, 67);
        Point offset = originalBaseOffset + (newOffset - newBaseOffset);
        const int tileIndex = 360;

        // Replace the ® with the subtitle
        Animation anim = BlackLumAndLogoObj.GetReplacedAnimation(7);
        int channelIndex = 0;
        for (int frameIndex = 0; frameIndex < anim.FramesCount; frameIndex++)
        {
            for (int frameChannelIndex = 0; frameChannelIndex < anim.ChannelsPerFrame[frameIndex]; frameChannelIndex++)
            {
                AnimationChannel channel = anim.Channels[channelIndex];
                if (channel.ChannelType == AnimationChannelType.Sprite && channel.TileIndex == tileIndex)
                {
                    // Move in from bottom and bounce
                    if (frameIndex == 59)
                    {
                        channel.ObjectMode = OBJ_ATTR_ObjectMode.REG;
                        channel.TileIndex = (ushort)(baseTileIndex + letters.Length);
                        channel.XPosition = (short)offset.X;
                        channel.YPosition = (short)(offset.Y + 50);
                    }
                    else if (frameIndex == 60)
                    {
                        channel.ObjectMode = OBJ_ATTR_ObjectMode.REG;
                        channel.TileIndex = (ushort)(baseTileIndex + letters.Length);
                        channel.XPosition = (short)offset.X;
                        channel.YPosition = (short)offset.Y;
                    }
                    else if (frameIndex == 61)
                    {
                        channel.ObjectMode = OBJ_ATTR_ObjectMode.REG;
                        channel.TileIndex = (ushort)(baseTileIndex + letters.Length);
                        channel.XPosition = (short)offset.X;
                        channel.YPosition = (short)(offset.Y + 5);
                    }
                    else if (frameIndex == 62)
                    {
                        channel.ObjectMode = OBJ_ATTR_ObjectMode.REG;
                        channel.TileIndex = (ushort)(baseTileIndex + letters.Length);
                        channel.XPosition = (short)offset.X;
                        channel.YPosition = (short)offset.Y;
                    }
                }

                channelIndex++;
            }
        }

        foreach (AnimationChannel channel in BlackLumAndLogoObj.GetReplacedAnimation(8).Channels)
        {
            if (channel.TileIndex == tileIndex)
            {
                channel.ObjectMode = OBJ_ATTR_ObjectMode.REG;
                channel.TileIndex = (ushort)(baseTileIndex + letters.Length);
                channel.XPosition = (short)offset.X;
                channel.YPosition = (short)offset.Y;
            }
        }
    }

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
        if (Engine.ActiveConfig.Tweaks.UseModernMainMenu)
        {
            Menu = new ModernMenuAll(Rom.Platform switch
            {
                Platform.GBA => InitialMenuPage.Language,
                Platform.NGage => InitialMenuPage.NGage_FirstPage,
                _ => throw new UnsupportedPlatformException(),
            });
            ((ModernMenuAll)Menu).LoadGameInfo();
        }
        else
        {
            Menu = new MenuAll(Rom.Platform switch
            {
                Platform.GBA => InitialMenuPage.Language,
                Platform.NGage => InitialMenuPage.NGage_FirstPage,
                _ => throw new UnsupportedPlatformException(),
            });
            ((MenuAll)Menu).LoadGameInfo();
        }

        AnimationPlayer = new AnimationPlayer(true, SoundEventsManager.ProcessEvent);

        AnimatedObjectResource introAnimResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.IntroAnimations);

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
        
        // The wings wrap to the bottom for the first few frames. This is only noticeable on GBA, but happens on N-Gage too.
        if (Engine.ActiveConfig.Tweaks.FixBugs)
            BlackLumAndLogoObj.SetAnimationWrap(0, new Box(0, 0, 0, 126));

        if (Engine.ActiveConfig.Tweaks.FixBugs)
        {
            // The 3 of the logo wraps to the bottom when first appearing
            if (Rom.Platform == Platform.GBA)
                BlackLumAndLogoObj.SetAnimationWrap(7, new Box(0, 0, 0, 126));
            // The R of the logo wraps to the bottom when first appearing
            else if (Rom.Platform == Platform.NGage)
                BlackLumAndLogoObj.SetAnimationWrap(7, new Box(0, 0, 0, 127));
            else
                throw new UnsupportedPlatformException();
        }

        PlayfieldResource introPlayfield = Rom.LoadResource<PlayfieldResource>(Rayman3DefinedResource.IntroPlayfield);
        Playfield = TgxPlayfield.Load<TgxPlayfield2D>(introPlayfield);
        Playfield.RenderContext.SetFixedResolution(Rom.OriginalResolution);

        Gfx.ClearColor = Color.Black;

        Playfield.Camera.Position = Vector2.Zero;
        Playfield.Step();

        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
        {
            Playfield.TileLayers[SkyTileLayerId].Screen.IsEnabled = false;
            Playfield.TileLayers[MainTileLayerId].Screen.IsEnabled = false;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Playfield.TileLayers[SkyTileLayerId].Screen.IsEnabled = true;
            Playfield.TileLayers[MainTileLayerId].Screen.IsEnabled = true;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        if (Rom.Platform == Platform.NGage)
            Playfield.TileLayers[UbisoftLogoTileLayerId].Screen.IsEnabled = false;

        Playfield.TileLayers[CloudsTileLayerId].Screen.IsEnabled = false;

        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
        {
            TextureScreenRenderer renderer = ((TextureScreenRenderer)Playfield.TileLayers[CloudsTileLayerId].Screen.Renderer);
            Playfield.TileLayers[CloudsTileLayerId].Screen.Renderer = new IntroCloudsRenderer(renderer.Texture);
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

        if (Engine.ActiveConfig.Tweaks.UseReadvancedLogo)
            ReplaceLogo();
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

        if (Engine.ActiveConfig.Tweaks.AllowPrototypeCheats && JoyPad.IsButtonJustPressed(GbaInput.L))
        {
            FrameManager.SetNextFrame(new LevelSelect());
            Localization.SetLanguage(0);
            Random.SetSeed(GameTime.ElapsedFrames);

            if (Engine.LocalConfig.Tweaks.PlayCheatTriggerSound)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
        }
    }

    #endregion

    #region Steps

    private void Step_1()
    {
        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
        {
            Timer++;

            // Skip timer if on N-Gage with GBA effects enabled since the Ubisoft logo doesn't show
            if (Timer > 60 || Rom.Platform == Platform.NGage)
            {
                Timer = 0;
                CurrentStepAction = Step_2;

                Playfield.TileLayers[SkyTileLayerId].Screen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                Playfield.TileLayers[SkyTileLayerId].Screen.Alpha = AlphaCoefficient.None;

                Playfield.TileLayers[SkyTileLayerId].Screen.IsEnabled = true;
                Playfield.TileLayers[MainTileLayerId].Screen.IsEnabled = true;
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
        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
        {
            Timer++;

            if (Timer >= 68)
            {
                Timer = 0;
                CurrentStepAction = Step_3;

                Playfield.TileLayers[SkyTileLayerId].Screen.RenderOptions.BlendMode = BlendMode.None;
                Playfield.TileLayers[UbisoftLogoTileLayerId].Screen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                Playfield.TileLayers[UbisoftLogoTileLayerId].Screen.Alpha = AlphaCoefficient.Max;
            }
            else
            {
                Playfield.TileLayers[SkyTileLayerId].Screen.Alpha = AlphaCoefficient.FromGbaValue(Timer / 4f);
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

            Playfield.TileLayers[UbisoftLogoTileLayerId].Screen.RenderOptions.BlendMode = BlendMode.None;
            Playfield.TileLayers[CloudsTileLayerId].Screen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
            Playfield.TileLayers[CloudsTileLayerId].Screen.Alpha = AlphaCoefficient.None;

            Playfield.TileLayers[UbisoftLogoTileLayerId].Screen.IsEnabled = false;

            // Only show clouds on GBA
            if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
                Playfield.TileLayers[CloudsTileLayerId].Screen.IsEnabled = true;
        }
        else
        {
            Playfield.TileLayers[UbisoftLogoTileLayerId].Screen.Alpha = AlphaCoefficient.FromGbaValue(AlphaCoefficient.MaxGbaValue - (Timer / 4f));
        }

        // N-Gage allows the intro to be skipped from here
        if (Rom.Platform == Platform.NGage)
        {
            if (JoyPad.IsButtonJustPressed(Rayman3Input.IntroSkip))
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
            Gfx.GetScreen(CloudsTileLayerId).Offset = new Vector2(0, ScrollY);
        }

        if (ScrollY > 600 && AlphaTimer <= 0x80)
        {
            Playfield.TileLayers[CloudsTileLayerId].Screen.Alpha = AlphaCoefficient.FromGbaValue(AlphaTimer / 32f);
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

        if (JoyPad.IsButtonJustPressed(Rayman3Input.IntroSkip))
        {
            if (Rom.Platform == Platform.NGage || ScrollY <= 863)
                IsSkipping = true;
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

        if (JoyPad.IsButtonJustPressed(Rayman3Input.IntroSkip))
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
            Playfield.TileLayers[CloudsTileLayerId].Screen.Alpha = AlphaCoefficient.FromGbaValue(AlphaTimer / 32f);
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