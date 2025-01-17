using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

public class FrameMultiSideScroller : Frame, IHasScene, IHasPlayfield
{
    #region Constructor

    public FrameMultiSideScroller(MapId mapId)
    {
        GameInfo.SetNextMapId(mapId);
        PausedMachineId = 0;

        // NOTE: In the original game this defaults to 0, which is a bug. It makes the fists have the blend flag set
        //       when they shouldn't. In the original game this isn't noticeable because blending hasn't yet been
        //       enabled, but in this version it is because blending is managed per object instead of globally.
        InvisibleActorId = -1;
        
        UserInfo = null;
    }

    #endregion

    #region Public Properties

    public Scene2D Scene { get; set; }
    public Action CurrentStepAction { get; set; }

    public FadeControl SavedFadeControl { get; set; }

    public TransitionsFX TransitionsFX { get; set; }
    public UserInfoMulti2D UserInfo { get; set; }
    public PauseDialog PauseDialog { get; set; }

    public int PausedMachineId { get; set; }
    public int InvisibleActorId { get; set; }

    // N-Gage exclusive
    public AnimatedObject PauseSign { get; set; }
    public bool IsShowingPauseSign { get; set; }

    #endregion

    #region Interface Properties

    Scene2D IHasScene.Scene => Scene;
    TgxPlayfield IHasPlayfield.Playfield => Scene.Playfield;

    #endregion

    #region Pubic Methods

    public override void Init()
    {
        GameInfo.InitLevel(LevelType.Multiplayer);
        
        TransitionsFX = new TransitionsFX(false);
        TransitionsFX.FadeInInit(1 / 16f);

        LevelMusicManager.Init();
        MultiplayerManager.Init();

        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraSideScroller(x), 3, 1);

        if (Rom.Platform == Platform.NGage)
        {
            AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.NGageMultiplayerPauseSignAnimations);
            PauseSign = new AnimatedObject(resource, resource.IsDynamic)
            {
                IsFramed = true,
                CurrentAnimation = Localization.LanguageUiIndex,
                BgPriority = 0,
                ObjPriority = 2,
                ScreenPos = new Vector2(88, 104),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };

            IsShowingPauseSign = false;
        }

        UserInfo = new UserInfoMulti2D(Scene);
        Scene.AddDialog(UserInfo, false, false);

        PauseDialog = new PauseDialog(Scene);

        Scene.Init();

        // NOTE: The game saves palette information here, but we don't need to do that

        MultiplayerInfo.TagInfo.SpawnNewItem(Scene, false);

        Scene.Playfield.Step();

        if (Rom.Platform == Platform.NGage && GameInfo.MapId == MapId.NGageMulti_CaptureTheFlagTeamPlayer)
        {
            // TODO: Why does the game do this?
            Gfx.GetScreen(1).IsEnabled = false;
        }

        Scene.AnimationPlayer.Execute();

        GameInfo.PlayLevelMusic();
        CurrentStepAction = Step_Normal;
    }

    public override void UnInit()
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        Scene = null;

        GameInfo.StopLevelMusic();
        SoundEventsManager.StopAllSongs();
    }

    public override void Step()
    {
        MubState state = MultiplayerManager.Step();

        if (state == MubState.Connected && (Rom.Platform == Platform.NGage || !EndOfFrame))
        {
            // TODO: This code is very different on N-Gage

            if (MultiplayerManager.HasReadJoyPads())
            {
                GameTime.Resume();
                CurrentStepAction();
                MultiplayerManager.ReleaseJoyPads();
            }
            else
            {
                GameTime.Pause();
            }

            LevelMusicManager.Step();
        }
        else
        {
            SoundEventsManager.StopAllSongs();

            MenuAll.Page menuPage = Rom.Platform == Platform.GBA && EndOfFrame
                ? MenuAll.Page.Multiplayer
                : MenuAll.Page.MultiplayerLostConnection;
            FrameManager.SetNextFrame(new MenuAll(menuPage));

            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
            Gfx.Fade = 1;
        }
    }

    #endregion

    #region Steps

    public void Step_Normal()
    {
        Scene.Step();
        Scene.Playfield.Step();
        TransitionsFX.StepAll();
        Scene.AnimationPlayer.Execute();

        // Pause
        if (!UserInfo.IsGameOver && TransitionsFX.IsFadeInFinished)
        {
            for (int id = 0; id < RSMultiplayer.MaxPlayersCount; id++)
            {
                if (MultiJoyPad.IsButtonJustPressed(id, GbaInput.Start) || 
                    (Rom.Platform == Platform.NGage && MultiJoyPad.IsButtonJustPressed(id, GbaInput.Select)))
                {
                    PausedMachineId = id;
                    CurrentStepAction = Step_Pause_Init;
                    break;
                }
            }
        }
    }

    public void Step_Pause_Init()
    {
        UserInfo.IsPaused = true;
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

        if (Rom.Platform == Platform.NGage)
            NGage_0x4 = true;

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
        UserInfo.Draw(Scene.AnimationPlayer);
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
    }

    public void Step_Pause_UnInit()
    {
        Scene.RemoveLastDialog();

        if (Rom.Platform == Platform.NGage)
            NGage_0x4 = false;

        Scene.RefreshDialogs();

        // NOTE: The game restores palette information here, but we don't need to do that

        Scene.ProcessDialogs();

        if (Rom.Platform == Platform.NGage)
            NGage_0x4 = false;

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_Resume;
    }

    public void Step_Pause_Resume()
    {
        Gfx.FadeControl = SavedFadeControl;
        Gfx.Fade = 0;

        UserInfo.ProcessMessage(this, Message.UserInfo_Unpause);

        Scene.Step();
        SoundEventsManager.ResumeAllSongs();
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Normal;

        UserInfo.IsPaused = false;

        if (Rom.Platform == Platform.NGage)
        {
            // TODO: Set user info animation

            Scene.NGage_Flag_6 = true;
        }
    }

    #endregion
}