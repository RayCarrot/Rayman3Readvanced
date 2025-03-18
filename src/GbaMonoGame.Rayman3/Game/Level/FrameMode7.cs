using BinarySerializer.Nintendo.GBA;
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

    protected void ExtendMap(MapTile[] repeatSection, int repeatSectionWidth, int repeatSectionHeight)
    {
        // In the original game if you see outside the map then it wraps whatever is loaded in VRAM, which will usually
        // be leftover tiles from before. This isn't very noticeable due to the low resolution, but here it is. So instead
        // we want to define a section of the map to repeat/tile outside the main map.

        // Get the main map layer
        TgxRotscaleLayerMode7 rotScaleLayer = ((TgxPlayfieldMode7)Scene.Playfield).RotScaleLayers[0];

        // Create a new map, same size as the original, with the repeat pattern
        MapTile[] overflowTileMap = new MapTile[rotScaleLayer.Width * rotScaleLayer.Height];
        for (int y = 0; y < rotScaleLayer.Height; y++)
        {
            for (int x = 0; x < rotScaleLayer.Width; x++)
            {
                overflowTileMap[y * rotScaleLayer.Width + x] = repeatSection[(y % repeatSectionHeight) * repeatSectionWidth + (x % repeatSectionWidth)];
            }
        }

        // Create a renderer
        IScreenRenderer overflowRenderer = Scene.Playfield.GfxTileKitManager.CreateTileMapRenderer(
            renderOptions: rotScaleLayer.Screen.RenderOptions,
            animatedTilekitManager: Scene.Playfield.AnimatedTilekitManager,
            layerCachePointer: rotScaleLayer.Resource.Offset + 1, // A bit hacky, but we need a unique cache id for this
            width: rotScaleLayer.Width,
            height: rotScaleLayer.Height,
            tileMap: overflowTileMap,
            baseTileIndex: 512,
            is8Bit: rotScaleLayer.Is8Bit,
            isDynamic: false);

        // Create 9 maps to render in a 3x3 grid where the middle one is the original map
        MultiScreenRenderer.Section[] sections = new MultiScreenRenderer.Section[9];

        // Set the original map
        sections[0] = new MultiScreenRenderer.Section(rotScaleLayer.Screen.Renderer, Vector2.Zero);
        
        // Set the remaining 8 maps
        int i = 1;
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                // Ignore if the original map
                if (x == 0 && y == 0)
                    continue;

                sections[i] = new MultiScreenRenderer.Section(overflowRenderer, new Vector2(rotScaleLayer.PixelWidth * x, rotScaleLayer.PixelHeight * y));

                i++;
            }
        }

        // Replace the renderer
        rotScaleLayer.Screen.Renderer = new MultiScreenRenderer(sections, new Vector2(rotScaleLayer.PixelWidth * 3, rotScaleLayer.PixelHeight * 3));
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
        if (PauseDialog.DrawStep == PauseDialogDrawStep.Hide)
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