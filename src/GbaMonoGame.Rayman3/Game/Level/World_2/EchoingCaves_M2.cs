using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
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
        GameInfo.SetLevelRichPresence();

        // Custom for the time attack mode
        if (TimeAttackInfo.IsActive)
            TimeAttackInfo.InitLevel(GameInfo.MapId);

        CanPause = true;
        Fog = null;
        LevelMusicManager.Init();
        CircleTransitionScreenEffect = new CircleTransitionScreenEffect()
        {
            RenderContext = Engine.GameRenderContext,
        };
        
        TransitionsFX.Init(true);
        TransitionsFX.FadeInInit(4);

        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraSideScroller(x), 3, 0);

        // Add user info (default hud)
        UserInfo = new UserInfoSideScroller(Scene, GameInfo.GetLevelHasBlueLum());
        Scene.AddDialog(UserInfo, false, false);

        // Create pause dialog, but don't add yet
        PauseDialog = Engine.ActiveConfig.Tweaks.UseModernPauseDialog ? new ModernPauseDialog(Scene, !TimeAttackInfo.IsActive) : new PauseDialog(Scene);

        // Custom for the time attack mode
        if (TimeAttackInfo.IsActive)
        {
            // Add dialog for the HUD
            TimeAttackDialog = new TimeAttackDialog(Scene);
            Scene.AddDialog(TimeAttackDialog, false, false);

            // Add actors (time freeze items)
            foreach (ActorResource actorResource in TimeAttackInfo.GetActors())
                Scene.KnotManager.AddActor(Scene, actorResource, GameObjectType.AlwaysActor);

            Scene.KnotManager.AddPendingActors();
        }

        // Custom cheat dialog
        CheatDialog = new CheatDialog(Scene);

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

        // Fix the moving helicopter bomb cycles if all objects are active, since otherwise their cycles will be
        // arbitrary and could result in the section being impossible to complete. We fix this by disabling them
        // and adding captors to resurrect them at the exact same point they would normally spawn from the knots.
        if (Scene.KeepAllObjectsActive)
        {
            Vector2 captorSize = new(16, Scene.Playfield.PhysicalLayer.PixelHeight);
            if (Rom.Platform == Platform.GBA)
            {
                // Stop moving the bombs
                Scene.GetGameObject(50).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(51).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(52).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(47).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(48).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(49).ProcessMessage(this, Message.Readvanced_StopMoving);

                // Add captors for each point where the original game would load a new knot
                Scene.KnotManager.AddCaptor(Scene, new Box(new Vector2(7259, 0), captorSize),
                [
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 50, TriggerIterationIndex = 0 },
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 51, TriggerIterationIndex = 0 },
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 52, TriggerIterationIndex = 0 },
                ]);
                Scene.KnotManager.AddCaptor(Scene, new Box(new Vector2(7739, 0), captorSize),
                [
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 47, TriggerIterationIndex = 0 },
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 48, TriggerIterationIndex = 0 },
                ]);
                // For this last one we move it back 1 cycle (Rayman's speed is 3 and the cycle takes 306 frames) since
                // otherwise it'd be on screen before it gets triggered.
                Scene.KnotManager.AddCaptor(Scene, new Box(new Vector2(7979 - (3 * 306), 0), captorSize),
                [
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 49, TriggerIterationIndex = 0 },
                ]);
            }
            else if (Rom.Platform == Platform.NGage)
            {
                // Stop moving the bombs
                Scene.GetGameObject(56).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(57).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(58).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(54).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(53).ProcessMessage(this, Message.Readvanced_StopMoving);
                Scene.GetGameObject(55).ProcessMessage(this, Message.Readvanced_StopMoving);

                // NOTE: Sadly multiple bombs here are visible on screen before getting triggered

                // Add captors for each point where the original game would load a new knot
                Scene.KnotManager.AddCaptor(Scene, new Box(new Vector2(7260, 0), captorSize),
                [
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 56, TriggerIterationIndex = 0 },
                ]);
                Scene.KnotManager.AddCaptor(Scene, new Box(new Vector2(7437, 0), captorSize),
                [
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 57, TriggerIterationIndex = 0 },
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 58, TriggerIterationIndex = 0 },
                ]);
                Scene.KnotManager.AddCaptor(Scene, new Box(new Vector2(7788, 0), captorSize),
                [
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 54, TriggerIterationIndex = 0 },
                ]);
                Scene.KnotManager.AddCaptor(Scene, new Box(new Vector2(7965, 0), captorSize),
                [
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 53, TriggerIterationIndex = 0 },
                    new CaptorEvent { MessageId = (ushort)Message.Readvanced_ResumeMoving, Param = 55, TriggerIterationIndex = 0 },
                ]);
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }
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
            if (!Engine.LocalConfig.Display.DisableFlashingLights && 
                (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage))
                bgScreen.IsEnabled = false;

            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessIncrease);
            Gfx.Fade = Engine.LocalConfig.Display.DisableFlashingLights
                ? AlphaCoefficient.None
                : AlphaCoefficient.Max;
            lightningScreen.Offset = new Vector2(Random.GetNumber(16), Random.GetNumber(96));
            lightningScreen.IsEnabled = true;

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Thunder1_Mix04);
            return;
        }

        // Frame 1
        if (time == LightningTime + 1)
        {
            Gfx.Fade = Engine.LocalConfig.Display.DisableFlashingLights 
                ? AlphaCoefficient.None 
                : AlphaCoefficient.FromGbaValue(15);
            Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 2-7
        if (time >= LightningTime + 2 && time < LightningTime + 8)
        {
            Gfx.Fade = Engine.LocalConfig.Display.DisableFlashingLights 
                ? AlphaCoefficient.None
                : AlphaCoefficient.FromGbaValue((31 - (time - LightningTime)) / 2f);
            Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 8-15
        if (time >= LightningTime + 8 && time < LightningTime + 16)
        {
            bgScreen.IsEnabled = true;
            lightningScreen.IsEnabled = false;
            Gfx.Fade = Engine.LocalConfig.Display.DisableFlashingLights 
                ? AlphaCoefficient.None
                : AlphaCoefficient.FromGbaValue((31 - (time - LightningTime)) / 2f);
            Gfx.ClearColor = Color.White;
            return;
        }

        // Frame 16-30
        if (time >= LightningTime + 16 && time < LightningTime + 31)
        {
            Gfx.Fade = Engine.LocalConfig.Display.DisableFlashingLights
                ? AlphaCoefficient.None
                : AlphaCoefficient.FromGbaValue((31 - (time - LightningTime)) / 2f);
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