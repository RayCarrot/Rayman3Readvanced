using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class FrameNewPower : Frame, IHasScene, IHasPlayfield
{
    #region Constructor

    public FrameNewPower(MapId mapId)
    {
        OriginalMapId = GameInfo.MapId;
        GameInfo.SetNextMapId(mapId);
    }

    #endregion

    #region Public Properties

    public MapId OriginalMapId { get; }
    public Scene2D Scene { get; set; }
    public ushort Timer { get; set; }
    public bool HasStoppedMusic { get; set; }
    public bool HasRunFirstScan { get; set; } // N-Gage only

    #endregion

    #region Interface Properties

    Scene2D IHasScene.Scene => Scene;
    TgxPlayfield IHasPlayfield.Playfield => Scene.Playfield;

    #endregion

    #region Private Methods

    private void CheckForEndOfLevel()
    {
        // NOTE: Not sure why the N-Gage version forces the JoyPad to run an extra time when starting the replay?
        if (Rom.Platform == Platform.NGage && !HasRunFirstScan && JoyPad.IsInReplayMode)
        {
            HasRunFirstScan = true;
            JoyPad.Scan();
        }

        if ((Rom.Platform == Platform.GBA && JoyPad.IsReplayFinished) ||
            (Rom.Platform == Platform.NGage && HasRunFirstScan && JoyPad.IsReplayFinished))
        {
            Timer = 1;
            Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
        }
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        GameInfo.InitLevel(LevelType.Normal);

        LevelMusicManager.Init();
        TransitionsFX.Init(true);
        TransitionsFX.FadeInInit(1);
        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraSideScroller(x), 3, 1);

        Scene.Init();
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        GameInfo.PlayLevelMusic();

        Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
        ((Rayman)Scene.MainActor).ActionId = Rayman.Action.Walk_Right;

        Timer = 0;
        HasStoppedMusic = false;
        HasRunFirstScan = false;

        Scene.AddDialog(new TextBoxDialog(Scene), false, false);

        SoundEngineInterface.SetNbVoices(10);

        if (GameInfo.MapId == MapId.Power1)
        {
            if (Rom.Platform == Platform.GBA || Engine.Config.Tweaks.UseGbaEffectsOnNGage)
            {
                TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
                TextureScreenRenderer renderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
                cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(renderer.Texture, [56, 120, 227]);
            }
        }
        else if (GameInfo.MapId == MapId.Power2)
        {
            Scene.AddDialog(new FogDialog(Scene), false, false);
        }

        // Re-init with the original map id
        GameInfo.SetNextMapId(OriginalMapId);
        GameInfo.InitLevel(LevelType.Normal);
    }

    public override void UnInit()
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        Scene.UnInit();
        Scene = null;

        GameInfo.StopLevelMusic();
        SoundEventsManager.StopAllSongs();
        SoundEngineInterface.SetNbVoices(7);
    }

    public override void Step()
    {
        Scene.Step();
        Scene.Playfield.Step();
        TransitionsFX.StepAll();
        Scene.AnimationPlayer.Execute();
        LevelMusicManager.Step();

        if (Timer == 0)
        {
            CheckForEndOfLevel();
        }
        else
        {
            Timer++;

            if (!TransitionsFX.IsFadingOut)
            {
                // Begin fade out after 1 second
                if (Timer == 61)
                {
                    TransitionsFX.FadeOutInit(1);
                }
                else if (Timer > 61)
                {
                    // Stop music
                    if (!HasStoppedMusic)
                    {
                        SoundEventsManager.StopAllSongs();
                        HasStoppedMusic = true;
                    }
                    // End level
                    else
                    {
                        GameInfo.UpdateLastCompletedLevel();
                        GameInfo.Save(GameInfo.CurrentSlot);
                        GameInfo.LoadLevel(GameInfo.GetNextLevelId());
                    }
                }
            }
        }
    }

    #endregion
}