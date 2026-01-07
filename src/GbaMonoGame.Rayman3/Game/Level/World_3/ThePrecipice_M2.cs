using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class ThePrecipice_M2 : FrameSideScroller
{
    public ThePrecipice_M2(MapId mapId) : base(mapId) { }

    public byte RainScrollY { get; set; }
    public ushort LightningTime { get; set; }
    public ushort Timer { get; set; }

    public override void Init()
    {
        base.Init();

        LightningTime = (ushort)Random.GetNumber(127);
        Timer = 0;
        // NOTE: The RainScrollY value is not initialized, meaning it can start at anything

        // Make the rain semi-transparent
        GfxScreen rainScreen = Gfx.GetScreen(3);
        rainScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
        rainScreen.Alpha = AlphaCoefficient.FromGbaValue(6);
    }

    public override void Step()
    {
        base.Step();

        GfxScreen bgScreen = Gfx.GetScreen(0);
        GfxScreen rainScreen = Gfx.GetScreen(3);

        // Don't show rain if paused
        if (CurrentStepAction != Step_Normal)
        {
            rainScreen.IsEnabled = false;
            return;
        }

        // Scroll the rain
        Vector2 camPos = Scene.Playfield.Camera.Position;
        rainScreen.Offset = new Vector2(camPos.X, RainScrollY);
        RainScrollY -= 3;

        // Toggle rain visibility
        rainScreen.IsEnabled = (GameTime.ElapsedFrames & 2) == 0;

        if (Timer < 120 || CircleTransitionMode is TransitionMode.Out or TransitionMode.FinishedOut)
        {
            Timer++;
            bgScreen.IsEnabled = true;
            return;
        }

        Gfx.ClearColor = Color.White;

        uint time = GameTime.ElapsedFrames % 512;

        // Frame 0
        if (time == LightningTime)
        {
            // NOTE: The N-Gage version forgot to remove this code (which they did for The Echoing Caves 2), meaning it
            //       causes graphical glitches! But we remove it here since the draw buffer works differently.
            if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
                bgScreen.IsEnabled = false;

            // NOTE: The original game turns off the rain blending during the lightning, but we don't have to
            if (!Engine.ActiveConfig.Tweaks.VisualImprovements)
                rainScreen.RenderOptions.BlendMode = BlendMode.None;

            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessIncrease);
            Gfx.Fade = AlphaCoefficient.Max;

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Thunder1_Mix04);
            return;
        }

        // Frame 1
        if (time == LightningTime + 1)
        {
            Gfx.Fade = AlphaCoefficient.FromGbaValue(15);

            if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
                Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 2-7
        if (time >= LightningTime + 2 && time < LightningTime + 8)
        {
            Gfx.Fade = AlphaCoefficient.FromGbaValue((31 - (time - LightningTime)) / 2f);
            return;
        }

        // Frame 8-15
        if (time >= LightningTime + 8 && time < LightningTime + 16)
        {
            bgScreen.IsEnabled = true;
            Gfx.Fade = AlphaCoefficient.FromGbaValue((31 - (time - LightningTime)) / 2f);
            return;
        }

        // Frame 16-30
        if (time >= LightningTime + 16 && time < LightningTime + 31)
        {
            Gfx.Fade = AlphaCoefficient.FromGbaValue((31 - (time - LightningTime)) / 2f);
            return;
        }

        // Frame 31
        if (time == LightningTime + 31)
        {
            // Make the rain semi-transparent again
            rainScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
            rainScreen.Alpha = AlphaCoefficient.FromGbaValue(6);

            Gfx.FadeControl = FadeControl.None;

            if (Timer == 121 || (Random.GetNumber(31) & 0x10) == 0)
            {
                LightningTime = (ushort)(Random.GetNumber(359) + 120);
                Timer = LightningTime < 447 ? (ushort)120 : (ushort)121;
            }
            else
            {
                LightningTime += 32;
                Timer = 121;
            }
            return;
        }

        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
            Gfx.ClearColor = Color.White;
    }
}