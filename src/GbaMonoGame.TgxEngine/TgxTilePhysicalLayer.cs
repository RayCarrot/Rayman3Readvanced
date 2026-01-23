using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class TgxTilePhysicalLayer : TgxGameLayer
{
    public TgxTilePhysicalLayer(RenderContext renderContext, GameLayerResource gameLayerResource) : base(gameLayerResource)
    {
        RenderContext = renderContext;
        CollisionMap = gameLayerResource.PhysicalLayer.CollisionMap;
    }
    
    public RenderContext RenderContext { get; }
    public GfxScreen DebugScreen { get; set; } // Collision map screen for debugging
    public byte[] CollisionMap { get; }

    public void EnsureDebugScreenIsCreated()
    {
        if (DebugScreen == null)
        {
            DebugScreen = new GfxScreen(-1)
            {
                IsEnabled = false,
                Offset = Vector2.Zero,
                Priority = 0,
                Wrap = false,
                Is8Bit = null,
                Renderer = new CollisionMapScreenRenderer(Width, Height, CollisionMap),
                RenderContext = RenderContext,
            };
            Gfx.AddScreen(DebugScreen);
        }
    }

    public override void SetOffset(Vector2 offset)
    {
        if (DebugScreen != null)
            DebugScreen.Offset = offset;
    }

    public override void SetWorldViewProjMatrix(Matrix worldViewProj)
    {
        if (DebugScreen != null)
            DebugScreen.RenderOptions = DebugScreen.RenderOptions with { WorldViewProj = worldViewProj };
    }
}