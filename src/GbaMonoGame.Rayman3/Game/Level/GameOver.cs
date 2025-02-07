using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class GameOver : Frame
{
    public AnimationPlayer AnimationPlayer { get; set; }
    public TransitionsFX TransitionsFX { get; set; }
    public AnimatedObject Rayman { get; set; }
    public AnimatedObject Countdown1 { get; set; }
    public AnimatedObject Countdown2 { get; set; }
    public AnimatedObject Butterfly1 { get; set; }
    public AnimatedObject Butterfly2 { get; set; }

    public GameOverMode Mode { get; set; }
    public bool IsCountdownFacingRight { get; set; }
    public int Timer { get; set; }

    private void NextRaymanIdleAnimation()
    {
        int rand = Random.GetNumber(100);
        Rayman.CurrentAnimation = Rayman.CurrentAnimation switch
        {
            3 => rand >= 50 ? 5 : 4,
            4 => rand >= 50 ? 5 : 3,
            _ => rand >= 50 ? 4 : 3
        };
    }

    private bool IsPlayingIdleRaymanAnimation()
    {
        return Rayman.CurrentAnimation is 3 or 4 or 5;
    }

    private void DrawCountdown()
    {
        if (Timer > 60)
            return;

        if (Timer == 30 && Countdown1.CurrentAnimation != 0)
            Countdown2.CurrentAnimation = Countdown1.CurrentAnimation - 1;

        if (Timer < 30)
        {
            int value = Timer * 2;
            if (!IsCountdownFacingRight)
                value *= -1;

            Vector2 basePos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(104, 10),
                Platform.NGage => new Vector2(78, 10),
                _ => throw new UnsupportedPlatformException()
            };
            Countdown2.ScreenPos = basePos + new Vector2(MathHelpers.Sin256(value) * 30, MathHelpers.Cos256(value) * 30);

            float scale = Timer * 0.06246948242f + 1;
            Countdown2.AffineMatrix = new AffineMatrix(0, scale, scale);

            Countdown2.GbaAlpha = 15 - Timer / 2f;

            Countdown1.ObjPriority = 33;
            Countdown2.ObjPriority = 32;
        }
        else
        {
            Vector2 basePos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(104, 0),
                Platform.NGage => new Vector2(78, 0),
                _ => throw new UnsupportedPlatformException()
            };
            Countdown2.ScreenPos = basePos + new Vector2(0, Timer - 20);

            float scale = (60 - Timer) * 0.06246948242f + 1;
            Countdown2.AffineMatrix = new AffineMatrix(0, scale, scale);

            Countdown2.GbaAlpha = (Timer - 30) / 2f;

            Countdown1.ObjPriority = 32;
            Countdown2.ObjPriority = 33;
        }

        if (Mode == GameOverMode.Countdown && (Countdown2.CurrentAnimation != 9 || Timer < 30))
        {
            AnimationPlayer.Play(Countdown1);
        }

        if ((Mode == GameOverMode.Countdown || Timer < 30) &&
            (Countdown1.CurrentAnimation != 9 || Timer >= 30) &&
            (Countdown1.CurrentAnimation != 0 || Timer < 30))
        {
            AnimationPlayer.Play(Countdown2);
        }

        Timer++;

        if (Timer == 60 && Countdown2.CurrentAnimation == 9)
        {
            Timer = 0;
            Countdown1.CurrentAnimation = 9;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__GameOver_BeepFX01_Mix02);
        }
    }

    public override void Init()
    {
        TransitionsFX = new TransitionsFX(true);

        Gfx.AddScreen(new GfxScreen(2)
        {
            IsEnabled = true,
            Priority = 1,
            Offset = Vector2.Zero,
            Renderer = new TextureScreenRenderer(Engine.TextureCache.GetOrCreateObject(
                pointer: Rom.Loader.Rayman3_GameOverBitmap.Offset,
                id: 0,
                createObjFunc: static () => new BitmapTexture2D(
                    width: (int)Rom.OriginalResolution.X,
                    height: (int)Rom.OriginalResolution.Y,
                    bitmap: Rom.Loader.Rayman3_GameOverBitmap.ImgData,
                    palette: new Palette(Rom.Loader.Rayman3_GameOverPalette)))),
            RenderOptions = { RenderContext = Rom.OriginalGameRenderContext },
        });

        AnimationPlayer = new AnimationPlayer(false, SoundEventsManager.ProcessEvent);

        AnimatedObjectResource raymanAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.GameOverRaymanAnimations);
        AnimatedObjectResource countdownAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.GameOverCountdownAnimations);
        AnimatedObjectResource butterflyAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.GameOverButterflyAnimations);

        Rayman = new AnimatedObject(raymanAnimations, raymanAnimations.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 1,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(120, 150),
                Platform.NGage => new Vector2(90, 104),
                _ => throw new UnsupportedPlatformException()
            },
            RenderContext = Rom.OriginalGameRenderContext,
        };

        Countdown1 = new AnimatedObject(countdownAnimations, countdownAnimations.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 9,
            BgPriority = 0,
            ObjPriority = 32,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(104, 40),
                Platform.NGage => new Vector2(78, 40),
                _ => throw new UnsupportedPlatformException()
            },
            RenderContext = Rom.OriginalGameRenderContext,
        };
        Countdown2 = new AnimatedObject(countdownAnimations, countdownAnimations.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 9,
            BgPriority = 0,
            ObjPriority = 32,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(104, 40),
                Platform.NGage => new Vector2(78, 40),
                _ => throw new UnsupportedPlatformException()
            },
            RenderContext = Rom.OriginalGameRenderContext,
        };
        Countdown2.RenderOptions.Alpha = true;

        Butterfly1 = new AnimatedObject(butterflyAnimations, butterflyAnimations.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 8,
            BgPriority = 0,
            ObjPriority = 33,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(173, 41),
                Platform.NGage => new Vector2(120, 81),
                _ => throw new UnsupportedPlatformException()
            },
            RenderContext = Rom.OriginalGameRenderContext,
        };
        Butterfly2 = new AnimatedObject(butterflyAnimations, butterflyAnimations.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 7,
            BgPriority = 0,
            ObjPriority = 33,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(42, 140),
                Platform.NGage => new Vector2(30, 188),
                _ => throw new UnsupportedPlatformException()
            },
            RenderContext = Rom.OriginalGameRenderContext,
        };

        Mode = GameOverMode.Intro;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__tizetre_Swing);
        IsCountdownFacingRight = true;
        Timer = 0x3c;
    }

    public override void UnInit()
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__tizetre_Swing);

        Gfx.ClearColor = Color.Black;

        if (Mode == GameOverMode.ReturnToMenu)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
    }

    public override void Step()
    {
        switch (Mode)
        {
            case GameOverMode.Intro:
                if (Rayman.EndOfAnimation)
                {
                    if (Rayman.CurrentAnimation != 2)
                    {
                        Rayman.CurrentAnimation++;
                    }
                    else
                    {
                        NextRaymanIdleAnimation();
                        Mode = GameOverMode.Countdown;
                        Timer = 31;
                    }
                }
                break;

            case GameOverMode.Countdown:
                if (Rayman.EndOfAnimation)
                    NextRaymanIdleAnimation();

                if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
                    Mode = GameOverMode.Continue;
                }
                else if (Timer > 60)
                {
                    Timer = 0;
                    IsCountdownFacingRight = !IsCountdownFacingRight;
                    if (Countdown1.CurrentAnimation != 0)
                    {
                        Countdown2.CurrentAnimation = Countdown1.CurrentAnimation;
                        Countdown1.CurrentAnimation--;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__GameOver_BeepFX01_Mix02);
                    }
                    else
                    {
                        Countdown2.CurrentAnimation = 0;
                        Mode = GameOverMode.GameOver;
                    }
                }
                break;

            case GameOverMode.Continue:
                if (Rayman.EndOfAnimation)
                {
                    if (IsPlayingIdleRaymanAnimation())
                    {
                        Rayman.CurrentAnimation = 8;
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                    }
                    else if (Rayman.CurrentAnimation != 9)
                    {
                        Rayman.CurrentAnimation++;
                    }
                    else
                    {
                        Mode = GameOverMode.ReloadLevel;
                        TransitionsFX.FadeOutInit(2 / 16f);
                    }
                }
                break;

            case GameOverMode.GameOver:
                if (Rayman.EndOfAnimation)
                {
                    if (IsPlayingIdleRaymanAnimation())
                    {
                        Rayman.CurrentAnimation = 6;
                    }
                    else if (Rayman.CurrentAnimation != 7)
                    {
                        Rayman.CurrentAnimation++;
                    }
                    else
                    {
                        Mode = GameOverMode.ReturnToMenu;
                        TransitionsFX.FadeOutInit(2 / 16f);

                        SoundEventsManager.StopAllSongs();
                    }
                }
                else if (Rayman.CurrentAnimation == 7 && Rayman.CurrentFrame == 25 && !Rayman.IsDelayMode)
                {
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__death, 0);
                }
                break;

            case GameOverMode.ReloadLevel:
                if (TransitionsFX.IsFadeOutFinished)
                {
                    GameInfo.PersistentInfo.Lives = 3;
                    GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.MapId;
                    GameInfo.LoadLevel(GameInfo.MapId);
                }
                break;

            case GameOverMode.ReturnToMenu:
                if (TransitionsFX.IsFadeOutFinished)
                {
                    FrameManager.SetNextFrame(new MenuAll(MenuAll.Page.SelectGameMode));
                }
                break;
        }

        AnimationPlayer.Play(Butterfly1);
        AnimationPlayer.Play(Butterfly2);
        AnimationPlayer.Play(Rayman);
        DrawCountdown();

        TransitionsFX.StepAll();
        AnimationPlayer.Execute();
    }

    public enum GameOverMode
    {
        Intro = 0,
        Countdown = 1,
        Continue = 2,
        GameOver = 3,
        ReloadLevel = 4,
        ReturnToMenu = 5,
    }
}