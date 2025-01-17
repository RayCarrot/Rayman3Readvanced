using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

public class FrameMode7 : Frame, IHasScene, IHasPlayfield
{
    #region Constructor

    public FrameMode7(MapId mapId)
    {
        GameInfo.SetNextMapId(mapId);
        MultiplayerPauseFrame = 0;
    }

    #endregion

    #region Public Properties

    public Scene2D Scene { get; set; }
    public Action CurrentStepAction { get; set; }

    public TransitionsFX TransitionsFX { get; set; }
    public PauseDialog PauseDialog { get; set; }

    public bool CanPause { get; set; }
    public byte MultiplayerPauseFrame { get; set; }
    public uint Timer { get; set; }

    #endregion

    #region Interface Properties

    Scene2D IHasScene.Scene => Scene;
    TgxPlayfield IHasPlayfield.Playfield => Scene.Playfield;

    #endregion

    #region Protected Methods

    protected void CommonInit()
    {
        LevelMusicManager.Init();

        TransitionsFX = new TransitionsFX(true);
        TransitionsFX.FadeInInit(1 / 16f);
        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraMode7(x), 3, 1);

        // Create pause dialog, but don't add yet
        PauseDialog = new PauseDialog(Scene);

        Scene.Init();
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();

        if (!RSMultiplayer.IsActive)
            GameInfo.PlayLevelMusic();

        CanPause = false;
        CurrentStepAction = Step_Normal;
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        GameInfo.InitLevel(LevelType.Normal);
        Timer = 0;
        CommonInit();
    }

    public override void UnInit()
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        Scene = null;

        GameInfo.StopLevelMusic();
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Motor01_Mix12);
    }

    public override void Step()
    {
        CurrentStepAction();
    }

    #endregion

    #region Steps

    public void Step_Normal()
    {
        Scene.Step();

        Timer++;

        if (Timer == 100)
            CanPause = true;

        Scene.Playfield.Step();
        TransitionsFX.StepAll();
        Scene.AnimationPlayer.Execute();
        LevelMusicManager.Step();

        // TODO: Handle pausing
    }

    #endregion
}