using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class CavesOfBadDreams : FrameSideScroller
{
    public CavesOfBadDreams(MapId mapId) : base(mapId) { }

    private const bool ScaleSkulls = true;

    public bool UseSinWaveEffect { get; set; } // Custom

    public byte SineWavePhase { get; set; }
    public Vector2 Offset { get; set; }
    public FadeMode Mode { get; set; }
    public int Timer { get; set; }

    public override void Init()
    {
        base.Init();

        SineWavePhase = 0;
        Offset = Vector2.Zero;
        Mode = FadeMode.Invisible;
        Timer = 120;

        GfxScreen skullScreen = Gfx.GetScreen(1);
        skullScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
        skullScreen.GbaAlpha = 0;

        UseSinWaveEffect = Rom.Platform == Platform.GBA || Engine.Config.Tweaks.UseGbaEffectsOnNGage;

        if (UseSinWaveEffect)
            skullScreen.IsEnabled = false;
        else
            skullScreen.IsEnabled = true;

        if (!ScaleSkulls)
            skullScreen.RenderOptions.RenderContext = Scene.RenderContext;

        if (UseSinWaveEffect)
        {
            TextureScreenRenderer renderer = ((TextureScreenRenderer)skullScreen.Renderer);
            skullScreen.Renderer = new SineWaveRenderer(renderer.Texture)
            {
                Amplitude = 24
            };
        }
    }

    public override void Step()
    {
        base.Step();

        GfxScreen skullScreen = Gfx.GetScreen(1);

        // Don't show skull screen on GBA if transitioning or paused
        if (UseSinWaveEffect && 
            (CircleTransitionMode != TransitionMode.None || CurrentStepAction != Step_Normal))
        {
            skullScreen.IsEnabled = false;
            return;
        }

        if (UseSinWaveEffect)
            skullScreen.IsEnabled = true;

        Vector2 camPos = Scene.Playfield.Camera.Position;

        if (ScaleSkulls)
        {
            TgxCluster skullScreenCluster = ((TgxCamera2D)Scene.Playfield.Camera).GetCluster(2);
            camPos *= skullScreenCluster.RenderContext.Resolution / Scene.Resolution;
        }

        skullScreen.Offset = new Vector2(camPos.X % 256, camPos.Y % 256) + Offset;

        if (skullScreen.Renderer is SineWaveRenderer sineWave)
            sineWave.Phase = SineWavePhase;

        SineWavePhase += 2;

        // NOTE: The original game does 1 step every second frame
        Offset += new Vector2(-0.5f, 0.5f);

        switch (Mode)
        {
            case FadeMode.FadeIn:
                skullScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                skullScreen.GbaAlpha = (28 - Timer) / 4f;
                
                Timer--;
                
                if (Timer == 0)
                {
                    // The game doesn't do this, but since we use floats it will cause the value to be stuck at a fractional value, so force to the max
                    skullScreen.GbaAlpha = 7;

                    Timer = 120;
                    Mode = FadeMode.Visible;
                }
                break;
            
            case FadeMode.Visible:
                Timer--;

                if (Timer == 0)
                {
                    Timer = 28;
                    Mode = FadeMode.FadeOut;
                }
                break;
            
            case FadeMode.FadeOut:
                skullScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                skullScreen.GbaAlpha = Timer / 4f;
                
                Timer--;
                
                if (Timer == 0)
                {
                    // The game doesn't do this, but since we use floats it will cause the value to be stuck at a fractional value, so force to the min
                    skullScreen.GbaAlpha = 0;

                    Timer = Random.GetNumber(120) + 30;
                    Mode = FadeMode.Invisible;
                }
                break;
            
            case FadeMode.Invisible:
                Timer--;

                if (Timer == 0)
                {
                    Timer = 28;
                    Mode = FadeMode.FadeIn;
                }
                break;
            
            case FadeMode.TransitionOut:
                Timer -= 4;

                if (Timer <= 0)
                {
                    Rayman rayman = (Rayman)Scene.MainActor;
                    
                    if (rayman.FinishedMap)
                        rayman.Timer = 0;
                    else
                        rayman.Timer = 80;

                    InitNewCircleTransition(false);
                    
                    Mode = FadeMode.Ended;
                    Timer = 0;
                }

                skullScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                skullScreen.GbaAlpha = Timer / 4f;
                break;

            case FadeMode.Ended:
                // Do nothing
                break;

            default:
                throw new Exception("Invalid mode");
        }
    }

    public enum FadeMode
    {
        FadeIn = 0,
        Visible = 1,
        FadeOut = 2,
        Invisible = 3,
        TransitionOut = 4,
        Ended = 5,
    }
}