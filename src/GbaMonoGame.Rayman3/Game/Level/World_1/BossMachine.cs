using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class BossMachine : FrameSideScroller
{
    public BossMachine(MapId mapId) : base(mapId) { }

    private void InitExtendedBackground()
    {
        TgxPlayfield2D playfield = (TgxPlayfield2D)Scene.Playfield;
        TgxCamera2D camera = playfield.Camera;

        // Get the clusters
        TgxCluster mainCluster = camera.GetMainCluster();
        TgxCluster hatchCluster = Rom.Platform switch
        {
            Platform.GBA => camera.GetCluster(1),
            Platform.NGage => camera.GetCluster(0),
            _ => throw new UnsupportedPlatformException()
        };

        // Get the layers
        TgxTileLayer originalLayer = playfield.TileLayers[0];
        TgxTileLayer hatchLayer = playfield.TileLayers[1];
        TgxTileLayer cratesLayer = playfield.TileLayers[2];

        // Load the texture for the extended background layer
        string extendedLayerTextureName = Rom.Platform switch
        {
            Platform.GBA => Assets.BossMachine_ExtendedLayer0_GBA,
            Platform.NGage => Assets.BossMachine_ExtendedLayer0_NGage,
            _ => throw new UnsupportedPlatformException()
        };
        Texture2D extendedLayerTexture = Engine.FrameContentManager.Load<Texture2D>(extendedLayerTextureName);

        // Get the origin for the extended background layer
        Vector2 origin = Rom.Platform switch
        {
            Platform.GBA => new Vector2(0, -40),
            Platform.NGage => new Vector2(0, -4),
            _ => throw new UnsupportedPlatformException()
        };

        // Create the layer
        TgxTextureLayer extendedLayer = new(
            renderContext: Scene.RenderContext, 
            texture: extendedLayerTexture, 
            layerId: 4, 
            priority: originalLayer.Screen.Priority)
        {
            Origin = origin
        };

        // Add the layer to the main cluster
        camera.AddLayer(0, extendedLayer);

        // Update sizes
        mainCluster.RecalculateSize();
        playfield.RenderContext.MaxResolution = mainCluster.Size;

        // Move the hatch into the main cluster so it scrolls with the new size
        if (hatchCluster != mainCluster)
        {
            hatchCluster.RemoveLayer(hatchLayer);
            mainCluster.AddLayer(hatchLayer);
        }

        // Disable wrapping since the playfield is bigger now
        hatchLayer.Screen.Wrap = false;
        cratesLayer.Screen.Wrap = false;

        // Disable the original background layer and remove from the main cluster
        mainCluster.RemoveLayer(originalLayer);
        originalLayer.Screen.IsEnabled = false;
    }

    public override void Init()
    {
        base.Init();

        TextBoxDialog textBox = new(Scene);
        Scene.AddDialog(textBox, false, false);
        textBox.SetCutsceneCharacter(TextBoxCutsceneCharacter.Murfy);
        textBox.SetText(13);

        if (Engine.Config.UseExtendedBackgrounds)
            InitExtendedBackground();
    }

    public override void Step()
    {
        base.Step();

        TgxPlayfield2D playfield = (TgxPlayfield2D)Scene.Playfield;
        TgxCluster cratesCluster = Rom.Platform switch
        {
            Platform.GBA => playfield.Camera.GetCluster(2),
            Platform.NGage => playfield.Camera.GetCluster(1),
            _ => throw new UnsupportedPlatformException()
        };

        // Update the size of the crates cluster based on the resolution so it stays at the bottom
        cratesCluster.Size = cratesCluster.Size with { Y = Scene.Resolution.Y + 8 };

        // The hatch is misaligned - optionally fix it
        if (Rom.Platform == Platform.NGage)
        {
            TgxTileLayer hatchLayer = playfield.TileLayers[1];
            hatchLayer.Origin = Engine.Config.FixBugs ? new Vector2(-8, 1) : Vector2.Zero;
        }
    }
}