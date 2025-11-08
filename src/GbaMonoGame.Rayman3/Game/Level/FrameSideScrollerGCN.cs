using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

// TODO: You can't see the lava in map 2 in modern resolution!
public class FrameSideScrollerGCN : FrameSideScroller
{
    public FrameSideScrollerGCN(GameCubeMapInfo mapInfo, GameCubeMap map, int gcnMapId) : base(default)
    {
        MapInfo = mapInfo;
        Map = map;
        GcnMapId = gcnMapId;
    }

    private const bool ScaleSkulls = true;

    public GameCubeMap Map { get; }

    public MapId PreviousMapId { get; set; }
    public Power PreviousPowers { get; set; }

    public int GcnMapId { get; }
    public GameCubeMapInfo MapInfo { get; }

    // Lava
    public byte LavaFadeOutTimer { get; set; }

    // Skull
    public byte SkullSineWavePhase { get; set; }
    public Vector2 SkullOffset { get; set; }
    public CavesOfBadDreams.FadeMode SkullMode { get; set; }
    public int SkullTimer { get; set; }

    // Lightning and rain
    public byte RainScrollY { get; set; }
    public ushort LightningTime { get; set; }
    public ushort LightningTimer { get; set; }

    public void RestoreMapAndPowers()
    {
        GameInfo.MapId = PreviousMapId;
        GameInfo.Powers = PreviousPowers;
    }

    public void FadeOut()
    {
        // NOTE: The original code here incorrectly checks for map 3!
        if (Engine.ActiveConfig.Tweaks.FixBugs)
        {
            if (GcnMapId == 2)
                LavaFadeOutTimer = 0;
        }
        else
        {
            if (GcnMapId == 3)
                LavaFadeOutTimer = 0;
        }
    }

    public override void Init()
    {
        GameInfo.InitLevel(LevelType.GameCube);

        PreviousMapId = GameInfo.MapId;
        GameInfo.MapId = MapId.GameCube_Bonus1 + GcnMapId;

        PreviousPowers = GameInfo.Powers;
        GameInfo.EnablePower(Power.All);

        // Optionally force GCN levels to show 0 lums and cages since they never have any
        if (Engine.ActiveConfig.Tweaks.FixBugs)
        {
            GameInfo.YellowLumsCount = 0;
            GameInfo.CagesCount = 0;
        }
        else
        {
            GameInfo.YellowLumsCount = MapInfo.LumsCount;
            GameInfo.CagesCount = MapInfo.CagesCount;
        }

        LevelMusicManager.Init();

        if (MapInfo.StartMusicSoundEvent != Rayman3SoundEvent.None)
            SoundEventsManager.ProcessEvent(MapInfo.StartMusicSoundEvent);

        CircleTransitionScreenEffect = new CircleTransitionScreenEffect()
        {
            RenderOptions = { RenderContext = Engine.GameRenderContext },
        };

        TransitionsFX.Init(true);
        Scene = new Scene2D(Map, x => new CameraSideScroller(x), 3, 1);

        // Add user info (default hud)
        UserInfo = new UserInfoSideScroller(Scene, MapInfo.HasBlueLum);
        UserInfo.ProcessMessage(this, Message.UserInfo_GameCubeLevel);

        // Create pause dialog, but don't add yet
        PauseDialog = Engine.ActiveConfig.Tweaks.UseModernPauseDialog ? new ModernPauseDialog(Scene, true) : new PauseDialog(Scene);

        Scene.AddDialog(UserInfo, false, false);
        Scene.Init();
        Scene.Playfield.Step();

        InitNewCircleTransition(true);

        Scene.AnimationPlayer.Execute();

        CanPause = true;
        Fog = null;
        LyTimer = null;
        CurrentStepAction = Step_Normal;

        switch (GcnMapId)
        {
            // Fog
            case 0:
                Fog = new FogDialog(Scene);
                Scene.AddDialog(Fog, false, false);
                break;
            
            // Scrolling clouds
            case 1:
                TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
                TextureScreenRenderer cloudsLayerRenderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
                cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(cloudsLayerRenderer.Texture, [32, 120, 227]);
                break;
            
            // Lava
            case 2:
                TgxTileLayer lavaLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
                TextureScreenRenderer lavaLayerRenderer;
                if (lavaLayer.Screen.Renderer is MultiScreenRenderer multiScreenRenderer)
                    lavaLayerRenderer = (TextureScreenRenderer)multiScreenRenderer.Sections[0].ScreenRenderer;
                else
                    lavaLayerRenderer = (TextureScreenRenderer)lavaLayer.Screen.Renderer;

                lavaLayer.Screen.Renderer = new SanctuaryLavaRenderer(lavaLayerRenderer.Texture);

                LavaFadeOutTimer = 0xFF;
                break;

            // Skull
            case 3 or 6:
                SkullSineWavePhase = 0;
                SkullOffset = Vector2.Zero;
                SkullMode = CavesOfBadDreams.FadeMode.Invisible;
                SkullTimer = 120;

                GfxScreen skullScreen = Gfx.GetScreen(1);
                skullScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                skullScreen.Alpha = AlphaCoefficient.None;

                if (!ScaleSkulls)
                    skullScreen.RenderOptions.RenderContext = Scene.RenderContext;

                TextureScreenRenderer renderer = ((TextureScreenRenderer)skullScreen.Renderer);
                skullScreen.Renderer = new SineWaveRenderer(renderer.Texture)
                {
                    Amplitude = 24
                };

                // NOTE: There's a bug where the level data has alpha blending enabled, which conflicts with the code here!
                if (Engine.ActiveConfig.Tweaks.FixBugs)
                    TransitionsFX.Screns.Remove(skullScreen.Id);
                break;

            // Lightning and rain
            case 4:
                LightningTime = (ushort)Random.GetNumber(127);
                LightningTimer = 0;
                // NOTE: The RainScrollY value is not initialized, meaning it can start at anything

                // Make the rain semi-transparent
                GfxScreen rainScreen = Gfx.GetScreen(3);
                rainScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                rainScreen.Alpha = AlphaCoefficient.FromGbaValue(6);
                break;
        }
    }

    public override void Step()
    {
        base.Step();

        switch (GcnMapId)
        {
            // Lava
            case 2:
            {
                Vector2 camPos = Scene.Playfield.Camera.Position;
                TgxTileLayer lavaLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];

                lavaLayer.Screen.Offset = lavaLayer.Screen.Offset with { Y = camPos.Y * MathHelpers.FromFixedPoint(0x7332) };

                if (CircleTransitionMode == TransitionMode.None && CurrentStepAction == Step_Normal)
                    ((SanctuaryLavaRenderer)lavaLayer.Screen.Renderer).SinValue++;

                // Unused due to a bug
                if (LavaFadeOutTimer != 0xFF)
                {
                    if (LavaFadeOutTimer < 16)
                    {
                        LavaFadeOutTimer++;

                        foreach (GfxScreen screen in Gfx.Screens)
                        {
                            if (screen.RenderOptions.BlendMode != BlendMode.None)
                                screen.Alpha = AlphaCoefficient.FromGbaValue(AlphaCoefficient.MaxGbaValue - LavaFadeOutTimer);
                        }
                    }

                    if (LavaFadeOutTimer == 6)
                    {
                        if (Engine.ActiveConfig.Tweaks.FixBugs)
                            ((Rayman)Scene.MainActor).Timer = 0;

                        InitNewCircleTransition(false);
                    }
                }

                break;
            }

            // Skull
            case 3 or 6:
            {
                GfxScreen skullScreen = Gfx.GetScreen(1);

                // Don't show skull screen if transitioning or paused
                if (CircleTransitionMode != TransitionMode.None || CurrentStepAction != Step_Normal)
                {
                    skullScreen.IsEnabled = false;
                    return;
                }

                skullScreen.IsEnabled = true;

                Vector2 camPos = Scene.Playfield.Camera.Position;

                if (ScaleSkulls)
                {
                    TgxCluster skullScreenCluster = ((TgxCamera2D)Scene.Playfield.Camera).GetCluster(2);
                    camPos *= skullScreenCluster.RenderContext.Resolution / Scene.Resolution;
                }

                skullScreen.Offset = new Vector2(camPos.X % 256, camPos.Y % 256) + SkullOffset;

                if (skullScreen.Renderer is SineWaveRenderer sineWave)
                    sineWave.Phase = SkullSineWavePhase;

                SkullSineWavePhase += 2;

                // NOTE: The original game does 1 step every second frame
                SkullOffset += new Vector2(-0.5f, 0.5f);

                switch (SkullMode)
                {
                    case CavesOfBadDreams.FadeMode.FadeIn:
                        skullScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                        skullScreen.Alpha = AlphaCoefficient.FromGbaValue((28 - SkullTimer) / 4f);

                        SkullTimer--;

                        if (SkullTimer == 0)
                        {
                            // The game doesn't do this, but since we use floats it will cause the value to be stuck at a fractional value, so force to the max
                            skullScreen.Alpha = AlphaCoefficient.FromGbaValue(7);

                            SkullTimer = 120;
                            SkullMode = CavesOfBadDreams.FadeMode.Visible;
                        }
                        break;

                    case CavesOfBadDreams.FadeMode.Visible:
                        SkullTimer--;

                        if (SkullTimer == 0)
                        {
                            SkullTimer = 28;
                            SkullMode = CavesOfBadDreams.FadeMode.FadeOut;
                        }
                        break;

                    case CavesOfBadDreams.FadeMode.FadeOut:
                        skullScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                        skullScreen.Alpha = AlphaCoefficient.FromGbaValue(SkullTimer / 4f);

                        SkullTimer--;

                        if (SkullTimer == 0)
                        {
                            // The game doesn't do this, but since we use floats it will cause the value to be stuck at a fractional value, so force to the min
                            skullScreen.Alpha = AlphaCoefficient.None;

                            SkullTimer = Random.GetNumber(120) + 30;
                            SkullMode = CavesOfBadDreams.FadeMode.Invisible;
                        }
                        break;

                    case CavesOfBadDreams.FadeMode.Invisible:
                        SkullTimer--;

                        if (SkullTimer == 0)
                        {
                            SkullTimer = 28;
                            SkullMode = CavesOfBadDreams.FadeMode.FadeIn;
                        }
                        break;

                    default:
                        throw new Exception("Invalid mode");
                }
                break;
            }

            // Lightning and rain
            case 4:
            {
                GfxScreen bgScreen = Gfx.GetScreen(0);
                GfxScreen rainScreen = Gfx.GetScreen(3);

                // NOTE: The lightning and rain code shouldn't run when paused, but they forgot to add a check for it! This causes
                //       different bugs depending on when you pause, such as continues thunder sounds and a white screen.
                if (Engine.ActiveConfig.Tweaks.FixBugs && CurrentStepAction != Step_Normal)
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

                if (LightningTimer < 120 || CircleTransitionMode is TransitionMode.Out or TransitionMode.FinishedOut)
                {
                    LightningTimer++;
                    bgScreen.IsEnabled = true;
                    return;
                }

                Gfx.ClearColor = Color.White;

                uint time = GameTime.ElapsedFrames % 512;

                // Frame 0
                if (time == LightningTime)
                {
                    bgScreen.IsEnabled = false;

                    // NOTE: The original game turns off the rain blending during the lightning, but we don't have to
                    if (!Engine.ActiveConfig.Tweaks.VisualImprovements)
                        rainScreen.RenderOptions.BlendMode = BlendMode.None;

                    Gfx.FadeControl = new FadeControl(FadeMode.BrightnessIncrease);
                    Gfx.Fade = 1;

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
                    return;
                }

                // Frame 8-15
                if (time >= LightningTime + 8 && time < LightningTime + 16)
                {
                    bgScreen.IsEnabled = true;
                    Gfx.GbaFade = (31 - (time - LightningTime)) / 2f;
                    return;
                }

                // Frame 16-30
                if (time >= LightningTime + 16 && time < LightningTime + 31)
                {
                    Gfx.GbaFade = (31 - (time - LightningTime)) / 2f;
                    return;
                }

                // Frame 31
                if (time == LightningTime + 31)
                {
                    // Make the rain semi-transparent again
                    rainScreen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                    rainScreen.Alpha = AlphaCoefficient.FromGbaValue(6);

                    Gfx.FadeControl = FadeControl.None;

                    if (LightningTimer == 121 || (Random.GetNumber(31) & 0x10) == 0)
                    {
                        LightningTime = (ushort)(Random.GetNumber(359) + 120);
                        LightningTimer = LightningTime < 447 ? (ushort)120 : (ushort)121;
                    }
                    else
                    {
                        LightningTime += 32;
                        LightningTimer = 121;
                    }
                    return;
                }

                Gfx.ClearColor = Color.White;
                break;
            }
        }
    }
}