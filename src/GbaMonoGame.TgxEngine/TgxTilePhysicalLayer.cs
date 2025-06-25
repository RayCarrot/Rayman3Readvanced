using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class TgxTilePhysicalLayer : TgxGameLayer
{
    public TgxTilePhysicalLayer(RenderContext renderContext, GameLayerResource gameLayerResource) : base(gameLayerResource)
    {
        CollisionMap = gameLayerResource.PhysicalLayer.CollisionMap;

        if (Engine.Config.Debug.DebugModeEnabled)
        {
            // Collision map screen for debugging
            DebugScreen = new GfxScreen(-1)
            {
                IsEnabled = false,
                Offset = Vector2.Zero,
                Priority = 0,
                Wrap = false,
                Is8Bit = null,
                Renderer = new CollisionMapScreenRenderer(Width, Height, CollisionMap),
                RenderOptions = { RenderContext = renderContext },
            };
            Gfx.AddScreen(DebugScreen);
        }
    }

    public GfxScreen DebugScreen { get; }
    public byte[] CollisionMap { get; }

    public void ToggleScreenVisibility()
    {
        if (DebugScreen != null)
            DebugScreen.IsEnabled = !DebugScreen.IsEnabled;
    }

    public override void SetOffset(Vector2 offset)
    {
        if (DebugScreen != null)
            DebugScreen.Offset = offset;
    }

    public override void SetWorldViewProjMatrix(Matrix worldViewProj)
    {
        if (DebugScreen != null)
            DebugScreen.RenderOptions.WorldViewProj = worldViewProj;
    }
}