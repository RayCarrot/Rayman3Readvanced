using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
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

    public Dialog UserInfo { get; set; }
    public Dialog PauseDialog { get; set; }

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

        TransitionsFX.Init(true);
        TransitionsFX.FadeInInit(1);
        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraMode7(x), 3, 1);

        // Create pause dialog, but don't add yet
        PauseDialog = Engine.Config.UseModernPauseDialog ? new ModernPauseDialog(Scene, true) : new PauseDialog(Scene);

        Scene.Init();
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();

        if (!RSMultiplayer.IsActive)
            GameInfo.PlayLevelMusic();

        CanPause = false;
        CurrentStepAction = Step_Normal;
    }

    protected void ExtendMap(MapTile[] repeatSection, int repeatSectionWidth, int repeatSectionHeight, int overrideMapWidth = -1, int overrideMapHeight = -1)
    {
        // In the original game if you see outside the map then it wraps whatever is loaded in VRAM, which will usually
        // be leftover tiles from before. This isn't very noticeable due to the low resolution, but here it is. So instead
        // we want to define a section of the map to repeat/tile outside the main map.

        // Get the main map layer
        TgxRotscaleLayerMode7 rotScaleLayer = ((TgxPlayfieldMode7)Scene.Playfield).RotScaleLayers[0];

        // Get the dimensions
        int mapWidth = overrideMapWidth != -1 ? overrideMapWidth : rotScaleLayer.Width;
        int mapHeight = overrideMapHeight != -1 ? overrideMapHeight : rotScaleLayer.Height;
        int mapPixelWidth = mapWidth * Tile.Size;
        int mapPixelHeight = mapHeight * Tile.Size;

        // Create a new map, same size as the original, with the repeat pattern
        MapTile[] overflowTileMap = new MapTile[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                overflowTileMap[y * mapWidth + x] = repeatSection[(y % repeatSectionHeight) * repeatSectionWidth + (x % repeatSectionWidth)];
            }
        }

        // Create a renderer
        IScreenRenderer overflowRenderer = Scene.Playfield.GfxTileKitManager.CreateTileMapRenderer(
            renderOptions: rotScaleLayer.Screen.RenderOptions,
            animatedTilekitManager: Scene.Playfield.AnimatedTilekitManager,
            layerCachePointer: rotScaleLayer.Resource.Offset + 1, // A bit hacky, but we need a unique cache id for this
            width: mapWidth,
            height: mapHeight,
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

                sections[i] = new MultiScreenRenderer.Section(overflowRenderer, new Vector2(mapPixelWidth * x, mapPixelHeight * y));

                i++;
            }
        }

        // Replace the renderer
        rotScaleLayer.Screen.Renderer = new MultiScreenRenderer(sections, new Vector2(mapPixelWidth * 3, mapPixelHeight * 3));
    }

    protected void SetBackgroundColor(Color color)
    {
        // The game doesn't do this, but depending on how things get scaled on screen you might see a tiny gap between
        // the map and the background. So we make sure it has a color that blends in rather than being black.
        Gfx.ClearColor = color;
    }

    // TODO: Make optional
    // TODO: Fix bumper positions so they don't overlap with walls
    protected void AddWalls(Point wallPoint, Point wallSize)
    {
        // Create the renderer
        Mode7WallsScreenRenderer wallsScreenRenderer = new((TgxPlayfieldMode7)Scene.Playfield, wallPoint, wallSize, 1.5f);

        // Create the screen and use ID 6 (5 is used for the fog)
        GfxScreen wallsScreen = new(6)
        {
            Priority = 0,
            Wrap = false,
            Offset = Vector2.Zero,
            IsEnabled = true,
            Renderer = wallsScreenRenderer,
            RenderOptions = { RenderContext = Scene.RenderContext }
        };

        // Add the screen
        Gfx.AddScreen(wallsScreen);
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
            for (int id = 0; id < RSMultiplayer.MaxPlayersCount; id++)
            {
                if (MultiJoyPad.IsButtonJustPressed(id, GbaInput.Start) && CanPause && !((UserInfoMultiMode7)UserInfo).IsGameOver)
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