using BinarySerializer.Ubisoft.GbaEngine;
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
        PausedMachineId = 0;
    }

    #endregion

    #region Public Properties

    public Scene2D Scene { get; set; }
    public Action CurrentStepAction { get; set; }

    public FadeControl SavedFadeControl { get; set; }

    public TransitionsFX TransitionsFX { get; set; }
    public Dialog UserInfo { get; set; }
    public PauseDialog PauseDialog { get; set; }

    public bool CanPause { get; set; }
    public int PausedMachineId { get; set; }
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

    public bool IsPaused()
    {
        return CurrentStepAction != Step_Normal;
    }

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

        Scene.UnInit();
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

        if (!RSMultiplayer.IsActive)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Start) && CanPause)
            {
                CurrentStepAction = Step_Pause_Init;
                GameTime.Pause();
            }
        }
        else
        {
            // TODO: Implement
        }
    }

    public void Step_Pause_Init()
    {
        SavedFadeControl = Gfx.FadeControl;

        // Fade after drawing screen 0, thus only leaving the sprites 0 as not faded
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease, FadeFlags.Screen0);
        Gfx.GbaFade = 6;

        UserInfo.ProcessMessage(this, Message.UserInfo_Pause);

        SoundEventsManager.FinishReplacingAllSongs();
        SoundEventsManager.PauseAllSongs();

        Scene.ProcessDialogs();
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_AddDialog;
    }

    public void Step_Pause_AddDialog()
    {
        PauseDialog.PausedMachineId = PausedMachineId;

        Scene.AddDialog(PauseDialog, true, false);

        Scene.Step();
        UserInfo.Draw(Scene.AnimationPlayer);
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_Paused;
    }

    public void Step_Pause_Paused()
    {
        if (PauseDialog.DrawStep == PauseDialog.PauseDialogDrawStep.Hide)
            CurrentStepAction = Step_Pause_UnInit;

        Scene.Step();

        // The original game doesn't have this check, but since we're still running the game loop
        // while in the simulated sleep mode we have to make sure to not draw the HUD then
        if (!PauseDialog.IsInSleepMode)
            UserInfo.Draw(Scene.AnimationPlayer);

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
    }

    public void Step_Pause_UnInit()
    {
        Scene.RemoveLastDialog();

        Scene.RefreshDialogs();
        Scene.ProcessDialogs();

        // We probably don't need to do this, but in the original game it needs to reload things like
        // palette indexes since it might be allocated differently in VRAM after unpausing.
        foreach (GameObject gameObj in Scene.KnotManager.GameObjects)
            gameObj.ProcessMessage(this, Message.ReloadAnimation);

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_Resume;
    }

    public void Step_Pause_Resume()
    {
        Gfx.FadeControl = SavedFadeControl;
        Gfx.Fade = 0;

        UserInfo.ProcessMessage(this, Message.UserInfo_Unpause);

        SoundEventsManager.ResumeAllSongs();
        Scene.Step();
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Normal;
        GameTime.Resume();
    }

    #endregion
}