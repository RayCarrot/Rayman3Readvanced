using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class FrameMissileMultiMode7 : FrameMode7
{
    public FrameMissileMultiMode7(MapId mapId, int lapsCount) : base(mapId)
    {
        LapsCount = lapsCount;
    }

    public new UserInfoMultiMode7 UserInfo
    {
        get => (UserInfoMultiMode7)base.UserInfo;
        set => base.UserInfo = value;
    }

    public int LapsCount { get; }
    public RaceManagerMulti RaceManager { get; set; }

    public GfxScreen FogScreen { get; set; }
    public Mode7RedFogScreenRenderer FogScreenRenderer { get; set; }
    public int ColorAdd { get; set; }
    public int ColorAddDelta { get; set; }

    private void InitFog()
    {
        // NOTE: The game handles the fog by updating the backdrop color based on the following scanlines and then blends it with the screen

        FogScreenRenderer = new Mode7RedFogScreenRenderer(
        [
            new Mode7RedFogScreenRenderer.FogLine(0x4, 0x00), // NOTE: Defined as scanline 0x14 in-game, but we want it to start from the beginning
            new Mode7RedFogScreenRenderer.FogLine(0x5, 0x1C),
            new Mode7RedFogScreenRenderer.FogLine(0x6, 0x22),
            new Mode7RedFogScreenRenderer.FogLine(0x7, 0x24),
            new Mode7RedFogScreenRenderer.FogLine(0x8, 0x28),
            new Mode7RedFogScreenRenderer.FogLine(0x9, 0x2C),
            new Mode7RedFogScreenRenderer.FogLine(0xA, 0x30),
            new Mode7RedFogScreenRenderer.FogLine(0xB, 0x34),
            new Mode7RedFogScreenRenderer.FogLine(0xC, 0x38),
            new Mode7RedFogScreenRenderer.FogLine(0xD, 0x3A),
            new Mode7RedFogScreenRenderer.FogLine(0xE, 0x3C),
            new Mode7RedFogScreenRenderer.FogLine(0xF, 0x3E),
            new Mode7RedFogScreenRenderer.FogLine(0xC, 0x42),
            new Mode7RedFogScreenRenderer.FogLine(0x9, 0x44),
            new Mode7RedFogScreenRenderer.FogLine(0x6, 0x46),
            new Mode7RedFogScreenRenderer.FogLine(0x4, 0x48),
        ]);

        FogScreen = new GfxScreen(5)
        {
            Priority = 0,
            Wrap = false,
            Is8Bit = null,
            Offset = Vector2.Zero,
            GbaAlpha = 12,
            IsEnabled = true,
            Renderer = FogScreenRenderer,
            RenderOptions = { BlendMode = BlendMode.Additive, RenderContext = Scene.RenderContext }
        };

        Gfx.AddScreen(FogScreen);
    }

    private void StepFog()
    {
        // NOTE: The game updates the fog color in a VSYNC callback

        // NOTE: The game doesn't show the fog while transitioning, but we can do that
        bool showFog = (!TransitionsFX.IsFadingIn && !TransitionsFX.IsFadingOut) ||
                       Engine.Config.Tweaks.VisualImprovements;
        if (showFog && !IsPaused())
        {
            FogScreen.IsEnabled = true;

            if (ColorAdd == 4)
                ColorAddDelta = -1;
            else if (ColorAdd == -4)
                ColorAddDelta = 1;

            if ((GameTime.ElapsedFrames & 3) == 0)
            {
                ColorAdd += ColorAddDelta;
                FogScreenRenderer.ColorAdd = ColorAdd;
            }
        }
        else
        {
            ColorAddDelta = 1;
            ColorAdd = 0;

            FogScreen.IsEnabled = false;
        }
    }

    public override void Init()
    {
        GameInfo.InitLevel(LevelType.Multiplayer);
        Timer = 0;
        CommonInit();
        UserInfo = new UserInfoMultiMode7(Scene);
        RaceManager = new RaceManagerMulti(Scene, UserInfo, LapsCount);
        Scene.AddDialog(UserInfo, false, false);

        // NOTE: The game does this in the level classes instead of this base class
        ColorAddDelta = -1;
        ColorAdd = 0;
        InitFog();
        SetBackgroundColor(new Color(139, 24, 24));
    }

    public override void Step()
    {
        base.Step();

        if (!IsPaused())
            RaceManager.Step();

        StepFog();
    }
}