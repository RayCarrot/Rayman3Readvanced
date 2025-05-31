using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class EchoingCaves_M2 : FrameSideScroller
{
    public EchoingCaves_M2(MapId mapId) : base(mapId) { }

    public ushort LightningTime { get; set; }
    public ushort Timer { get; set; }

    private void InitScene()
    {
        GameInfo.InitLevel(LevelType.Normal);

        CanPause = true;
        Fog = null;
        LevelMusicManager.Init();
        CircleTransitionScreenEffect = new CircleTransitionScreenEffect()
        {
            RenderOptions = { RenderContext = Engine.GameRenderContext },
        };
        
        TransitionsFX.Init(true);
        TransitionsFX.FadeInInit(4);

        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraSideScroller(x), 3, 0);

        // Add user info (default hud)
        UserInfo = new UserInfoSideScroller(Scene, GameInfo.GetLevelHasBlueLum());
        Scene.AddDialog(UserInfo, false, false);

        // Create pause dialog, but don't add yet
        PauseDialog = Engine.Config.UseModernPauseDialog ? new ModernPauseDialog(Scene, true) : new PauseDialog(Scene);

        Scene.Init();
        Scene.Playfield.Step();

        InitNewCircleTransition(true);

        Scene.AnimationPlayer.Execute();

        GameInfo.PlayLevelMusic();
        CurrentStepAction = Step_Normal;
    }

    public override void Init()
    {
        InitScene();

        LightningTime = (ushort)Random.GetNumber(127);
        Timer = 0;

        ((TgxPlayfield2D)Scene.Playfield).TileLayers[0].Screen.Priority = 3;
        ((TgxPlayfield2D)Scene.Playfield).TileLayers[1].Screen.Priority = 1;
        ((TgxPlayfield2D)Scene.Playfield).TileLayers[2].Screen.Priority = 0;
        ((TgxPlayfield2D)Scene.Playfield).TileLayers[3].Screen.Priority = 2;
    }

    public override void UnInit()
    {
        base.UnInit();

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__barrel);
    }

    public override void Step()
    {
        base.Step();

        // Get the relevant screens
        GfxScreen lightningScreen = Gfx.GetScreen(3);
        GfxScreen bgScreen = Gfx.GetScreen(0);

        // Don't show lightning if paused
        if (CurrentStepAction != Step_Normal)
        {
            bgScreen.IsEnabled = true;
            lightningScreen.IsEnabled = false;
            return;
        }

        if (Timer < 120 || CircleTransitionMode is TransitionMode.Out or TransitionMode.FinishedOut)
        {
            Timer++;
            Gfx.FadeControl = FadeControl.None;
            bgScreen.IsEnabled = true;
            lightningScreen.IsEnabled = false;
            return;
        }

        Gfx.ClearColor = Color.White;

        uint time = GameTime.ElapsedFrames % 512;

        // Frame 0
        if (time == LightningTime)
        {
            // N-Gage doesn't hide the background due to the brightness effect not being implemented
            if (Rom.Platform == Platform.GBA || Engine.Config.UseGbaEffectsOnNGage)
                bgScreen.IsEnabled = false;

            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessIncrease);
            Gfx.Fade = 1;
            lightningScreen.Offset = new Vector2(Random.GetNumber(16), Random.GetNumber(96));
            lightningScreen.IsEnabled = true;

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Thunder1_Mix04);
            return;
        }

        // Frame 1
        if (time == LightningTime + 1)
        {
            Gfx.GbaFade = 15;
            Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 2-7
        if (time >= LightningTime + 2 && time < LightningTime + 8)
        {
            Gfx.GbaFade = (31 - (time - LightningTime)) / 2f;
            Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 8-15
        if (time >= LightningTime + 8 && time < LightningTime + 16)
        {
            bgScreen.IsEnabled = true;
            lightningScreen.IsEnabled = false;
            Gfx.GbaFade = (31 - (time - LightningTime)) / 2f;
            Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 16-30
        if (time >= LightningTime + 16 && time < LightningTime + 31)
        {
            Gfx.GbaFade = (31 - (time - LightningTime)) / 2f;
            Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 31
        if (time == LightningTime + 31)
        {
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

        Gfx.ClearColor = Color.White;
    }
}