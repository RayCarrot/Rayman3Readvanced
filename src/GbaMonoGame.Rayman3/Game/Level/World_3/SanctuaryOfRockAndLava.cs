using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class SanctuaryOfRockAndLava : FrameSideScroller
{
    public SanctuaryOfRockAndLava(MapId mapId) : base(mapId) { }

    public byte FadeOutTimer { get; set; }

    public void FadeOut()
    {
        FadeOutTimer = 0;
    }

    public override void Init()
    {
        base.Init();

        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
        {
            TgxTileLayer lavaLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
            TextureScreenRenderer renderer;
            if (lavaLayer.Screen.Renderer is MultiScreenRenderer multiScreenRenderer)
                renderer = (TextureScreenRenderer)multiScreenRenderer.Sections[0].ScreenRenderer;
            else
                renderer = (TextureScreenRenderer)lavaLayer.Screen.Renderer;

            lavaLayer.Screen.Renderer = new SanctuaryLavaRenderer(renderer.Texture);

            FadeOutTimer = 0xFF;
        }
    }

    public override void Step()
    {
        base.Step();

        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
        {
            Vector2 camPos = Scene.Playfield.Camera.Position;
            TgxTileLayer lavaLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
            
            lavaLayer.Screen.Offset = lavaLayer.Screen.Offset with { Y = camPos.Y * MathHelpers.FromFixedPoint(0x7332) };

            if (CircleTransitionMode == TransitionMode.None && CurrentStepAction == Step_Normal)
                ((SanctuaryLavaRenderer)lavaLayer.Screen.Renderer).SinValue++;
        }

        if ((Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage) && FadeOutTimer != 0xFF)
        {
            if (FadeOutTimer < 16)
            {
                FadeOutTimer++;

                foreach (GfxScreen screen in Gfx.Screens)
                {
                    if (screen.BlendMode != BlendMode.None)
                        screen.Alpha = AlphaCoefficient.FromGbaValue(AlphaCoefficient.MaxGbaValue - FadeOutTimer);
                }
            }

            if (FadeOutTimer == 6)
            {
                ((Rayman)Scene.MainActor).Timer = 0;
                InitNewCircleTransition(false);
            }
        }
    }
}