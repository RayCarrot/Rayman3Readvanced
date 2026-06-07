using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    public byte[] CollisionMap { get; set; }

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
                Renderer = new CollisionMapScreenRenderer(
                    collisionTileSet: Engine.Assets.FixContentManager.Load<Texture2D>(Assets.Playfield.CollisionTileSet), 
                    tileSize: Tile.Size, 
                    width: Width, 
                    height: Height, 
                    collisionMap: CollisionMap),
                RenderContext = RenderContext,
            };
            Gfx.AddScreen(DebugScreen);
        }
    }

    public override void SetOffset(Vector2 offset)
    {
        DebugScreen?.Offset = offset;
    }

    public override void SetWorldViewProjMatrix(Matrix worldViewProj)
    {
        DebugScreen?.RenderOptions = DebugScreen.RenderOptions with { WorldViewProj = worldViewProj };
    }
}