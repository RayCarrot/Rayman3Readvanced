using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
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

    public UserInfoMulti2D UserInfo { get; set; }
    public Dialog PauseDialog { get; set; }

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
        
        TransitionsFX.Init(false);
        TransitionsFX.FadeInInit(1);

        LevelMusicManager.Init();
        MultiplayerManager.Init();

        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraSideScroller(x), 3, 1);

        if (Rom.Platform == Platform.NGage)
        {
            AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.NGageMultiplayerPauseSignAnimations);
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

        PauseDialog = Engine.Config.Tweaks.UseModernPauseDialog ? new ModernPauseDialog(Scene, false) : new PauseDialog(Scene);

        Scene.Init();

        // NOTE: The game saves palette information here, but we don't need to do that

        MultiplayerInfo.TagInfo.SpawnNewItem(Scene, false);

        Scene.Playfield.Step();

        // On N-Gage it hides the island/mountains background for one of the maps
        if (Rom.Platform == Platform.NGage && GameInfo.MapId == MapId.NGageMulti_CaptureTheFlagTeamPlayer)
            Gfx.GetScreen(1).IsEnabled = false;

        Scene.AnimationPlayer.Execute();

        GameInfo.PlayLevelMusic();
        CurrentStepAction = Step_Normal;
    }

    public override void UnInit()
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        Scene.UnInit();
        Scene = null;

        GameInfo.StopLevelMusic();
        SoundEventsManager.StopAllSongs();
    }

    public override void Step()
    {
        bool connected = MultiplayerManager.Step();

        if (connected && (Rom.Platform == Platform.NGage || !EndOfFrame))
        {
            if (Rom.Platform == Platform.GBA)
            {
                if (MultiplayerManager.HasReadJoyPads())
                {
                    GameTime.Resume();
                    CurrentStepAction();
                    MultiplayerManager.FrameProcessed();
                }
                else
                {
                    GameTime.Pause();
                }

                LevelMusicManager.Step();
            }
            else if (Rom.Platform == Platform.NGage)
            {
                if (!IsShowingPauseSign && MultiplayerManager.SyncTime != 0)
                {
                    // NOTE: The game loads the PauseSign animated object here
                }

                if (MultiplayerManager.SyncTime != 0)
                {
                    Scene.AnimationPlayer.PlayFront(PauseSign);

                    if (!IsShowingPauseSign)
                        ((NGageSoundEventsManager)SoundEventsManager.Current).PauseLoopingSoundEffects();

                    IsShowingPauseSign = true;
                }
                else
                {
                    if (IsShowingPauseSign)
                        ((NGageSoundEventsManager)SoundEventsManager.Current).ResumeLoopingSoundEffects();

                    IsShowingPauseSign = false;
                }

                if (MultiplayerManager.HasReadJoyPads())
                {
                    if (EndOfFrame)
                    {
                        if (MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && !((FrameMultiCaptureTheFlag)Current).IsMatchOver)
                        {
                            FrameManager.ReloadCurrentFrame();
                        }
                        else
                        {
                            FrameManager.SetNextFrame(new ModernMenuAll(InitialMenuPage.Multiplayer));

                            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                            Gfx.Fade = 1;
                        }
                    }
                    else
                    {
                        GameTime.Resume();

                        if (MultiplayerManager.PendingSystemSyncPause && CurrentStepAction == Step_Normal && !UserInfo.IsGameOver)
                        {
                            Current.ForcePauseFrame = false;
                            PausedMachineId = 0;
                            CurrentStepAction = Step_Pause_Init;
                        }

                        CurrentStepAction();
                        MultiplayerManager.FrameProcessed();
                        LevelMusicManager.Step();
                    }
                }
                else
                {
                    if (MultiplayerManager.SyncTime != 0)
                        Scene.AnimationPlayer.Execute();

                    GameTime.Pause();
                    LevelMusicManager.Step();
                }
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }
        else
        {
            SoundEventsManager.StopAllSongs();

            InitialMenuPage menuPage = Rom.Platform == Platform.GBA && EndOfFrame
                ? InitialMenuPage.Multiplayer
                : InitialMenuPage.MultiplayerLostConnection;
            FrameManager.SetNextFrame(new ModernMenuAll(menuPage));

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
        if (!UserInfo.IsGameOver && !TransitionsFX.IsFadingIn)
        {
            for (int id = 0; id < RSMultiplayer.MaxPlayersCount; id++)
            {
                if (Rom.Platform switch
                    {
                        Platform.GBA => MultiJoyPad.IsButtonJustPressed(id, GbaInput.Start),
                        Platform.NGage => NGageJoyPadHelpers.MultiIsSoftButtonJustPressed(id),
                        _ => throw new UnsupportedPlatformException()
                    })
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
        if (PauseDialog is PauseDialog pauseDialog)
            pauseDialog.PausedMachineId = PausedMachineId;
        else if (PauseDialog is ModernPauseDialog modernPauseDialog)
            modernPauseDialog.PausedMachineId = PausedMachineId;

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

        // NOTE: The game restores palette information here, but we don't need to do that

        Scene.ProcessDialogs();

        if (Rom.Platform == Platform.NGage)
            BlockPauseFrame = false;

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
            UserInfo.StartCountdownValue = 5;
            UserInfo.StartCountdown.CurrentAnimation = 3;

            Scene.IsMultiplayerPaused = true;
        }
    }

    #endregion
}