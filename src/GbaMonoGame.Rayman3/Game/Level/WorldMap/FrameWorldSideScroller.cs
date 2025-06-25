﻿using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

public abstract class FrameWorldSideScroller : Frame, IHasScene, IHasPlayfield
{
    #region Constructor

    protected FrameWorldSideScroller(MapId mapId)
    {
        GameInfo.SetNextMapId(mapId);
    }

    #endregion

    #region Public Properties

    public Scene2D Scene { get; set; }
    public Action CurrentStepAction { get; set; }

    public FadeControl SavedFadeControl { get; set; }

    public bool BlockPause { get; set; }
    public UserInfoWorldMap UserInfo { get; set; }
    public Dialog PauseDialog { get; set; }

    #endregion

    #region Interface Properties

    Scene2D IHasScene.Scene => Scene;
    TgxPlayfield IHasPlayfield.Playfield => Scene.Playfield;

    #endregion

    #region Pubic Methods

    public override void Init()
    {
        MapId prevMap = GameInfo.MapId;
        GameInfo.InitLevel(LevelType.Normal);
        LevelMusicManager.Init();

        TransitionsFX.Init(true);
        TransitionsFX.FadeInInit(1);

        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraSideScroller(x), 3, 1);

        // Set start position
        if (prevMap != MapId.WorldMap && GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
        {
            // Get the actor to spawn the main actor at (either default position or at a curtain)
            int startActorId = GameInfo.GetLevelCurtainActorId();

            Vector2 startPos = Scene.GetGameObject(startActorId).Position;
            startPos -= new Vector2(32, MathHelpers.Mod(startPos.Y, Tile.Size));

            while (Scene.GetPhysicalType(startPos) == PhysicalTypeValue.None)
                startPos += Tile.Down;

            Scene.MainActor.Position = startPos;
            Scene.Camera.SetFirstPosition();
        }

        // Create pause dialog, but don't add yet
        PauseDialog = Engine.Config.Tweaks.UseModernPauseDialog ? new ModernPauseDialog(Scene, false) : new PauseDialog(Scene);

        Scene.Init();
        // NOTE: The game calls vsync, steps the playfield and executes the animations here, but we do
        //       that in the derived classed instead since this is all to be run in one game frame.

        if (Rom.Platform == Platform.NGage || !SoundEventsManager.IsSongPlaying(GameInfo.GetLevelMusicSoundEvent()))
            GameInfo.PlayLevelMusic();

        BlockPause = false;
        CurrentStepAction = Step_Normal;
    }

    public override void UnInit()
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        Scene.UnInit();
        Scene = null;

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);
    }

    public override void Step()
    {
        CurrentStepAction();

        if (EndOfFrame)
            GameInfo.LoadLevel(GameInfo.GetNextLevelId());
    }

    #endregion

    #region Steps

    public void Step_Normal()
    {
        Scene.Step();
        Scene.Playfield.Step();
        TransitionsFX.StepAll();
        Scene.AnimationPlayer.Execute();
        LevelMusicManager.Step();

        if ((JoyPad.IsButtonJustPressed(GbaInput.Start) || (Rom.Platform == Platform.NGage && PauseFrame)) && 
            !BlockPause)
        {
            if (Rom.Platform == Platform.NGage)
                PauseFrame = false;

            CurrentStepAction = Step_Pause_Init;
            GameTime.Pause();
        }
    }

    public void Step_Pause_Init()
    {
        SavedFadeControl = Gfx.FadeControl;

        // Fade after drawing screen 0, thus only leaving the sprites 0 as not faded
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease, FadeFlags.Screen0);
        Gfx.GbaFade = 6;

        UserInfo.ProcessMessage(this, Message.UserInfo_Pause);

        Scene.ProcessDialogs();

        SoundEventsManager.FinishReplacingAllSongs();
        SoundEventsManager.PauseAllSongs();

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_AddDialog;
    }

    public void Step_Pause_AddDialog()
    {
        Scene.AddDialog(PauseDialog, true, false);

        if (Rom.Platform == Platform.NGage)
            BlockPauseFrame = true;

        Scene.Step();
        UserInfo.Draw(Scene.AnimationPlayer);
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_Paused;
    }

    public void Step_Pause_Paused()
    {
        if (PauseDialog is PauseDialog { DrawStep: PauseDialogDrawStep.Hide } or ModernPauseDialog { DrawStep: PauseDialogDrawStep.Hide })
            CurrentStepAction = Step_Pause_UnInit;

        Scene.Step();

        // The original game doesn't have this check, but since we're still running the game loop
        // while in the simulated sleep mode we have to make sure to not draw the HUD then
        if (PauseDialog is not PauseDialog { IsInSleepMode: true })
            UserInfo.Draw(Scene.AnimationPlayer);

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
    }

    public void Step_Pause_UnInit()
    {
        Scene.RemoveLastDialog();

        if (Rom.Platform == Platform.NGage)
            BlockPauseFrame = false;

        Scene.RefreshDialogs();
        Scene.ProcessDialogs();

        // We probably don't need to do this, but in the original game it needs to reload things like
        // palette indexes since it might be allocated differently in VRAM after unpausing.
        foreach (GameObject gameObj in Scene.KnotManager.GameObjects)
            gameObj.ProcessMessage(this, Message.Actor_ReloadAnimation);

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_Resume;
    }

    public void Step_Pause_Resume()
    {
        Gfx.FadeControl = SavedFadeControl;
        Gfx.Fade = 0;

        Scene.Step();
        
        UserInfo.ProcessMessage(this, Message.UserInfo_Unpause);

        SoundEventsManager.ResumeAllSongs();
        
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();

        if (Rom.Platform == Platform.NGage)
            BlockPauseFrame = false;

        CurrentStepAction = Step_Normal;
        GameTime.Resume();
    }

    #endregion
}