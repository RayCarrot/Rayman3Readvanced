using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Editor;

// TODO: Allow switching between Edit and Play mode. Play mode loads a Scene2D with the data from the EditableScene2D.
public class LevelEditor : Frame
{
    #region Constructor

    public LevelEditor(int mapId)
    {
        MapId = mapId;
    }

    #endregion

    #region Public Properties

    public int MapId { get; }
    public EditableScene2D Scene { get; set; }
    public TransitionsFX TransitionsFX { get; set; }
    public GfxScreen MapBackgroundScreen { get; set; }

    #endregion

    #region Private Methods

    private void UpdateMapBackground()
    {
        MapBackgroundScreen.Offset = Scene.Camera.Position;
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        TransitionsFX = new TransitionsFX(false);
        TransitionsFX.FadeInInit(2 / 16f);

        Scene = new EditableScene2D(MapId);

        // Change the clear color (background outside the map)
        Gfx.ClearColor = new Color(15, 14, 27);

        // Set a background to the map
        MapBackgroundScreen = new GfxScreen(0)
        {
            Priority = 3,
            IsEnabled = true,
            RenderContext = Scene.Camera.RenderContext,
            Renderer = new SolidColorScreenRenderer(Scene.MapSize, new Color(29, 27, 50))
        };
        Gfx.AddScreen(MapBackgroundScreen);

        Scene.Init();
        UpdateMapBackground();
        Scene.AnimationPlayer.Execute();
    }

    public override void UnInit()
    {
        Scene = null;

        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;
        Gfx.ClearColor = Color.Black;
    }

    public override void Step()
    {
        Scene.Step();
        UpdateMapBackground();

        TransitionsFX.StepAll();
        Scene.AnimationPlayer.Execute();
    }

    #endregion
}