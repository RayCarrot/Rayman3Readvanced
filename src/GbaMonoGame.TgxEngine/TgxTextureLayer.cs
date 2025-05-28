using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

// Custom class for extended layers
public class TgxTextureLayer : TgxGameLayer
{
    public TgxTextureLayer(RenderContext renderContext, Texture2D texture, int layerId, int priority, Vector2 origin) 
        : base(texture.Width / Tile.Size, texture.Height / Tile.Size)
    {
        Origin = origin;

        Screen = new GfxScreen(layerId)
        {
            IsEnabled = true,
            Offset = Vector2.Zero,
            Priority = priority,
            Wrap = true,
            Renderer = new TextureScreenRenderer(texture),
            RenderOptions = { RenderContext = renderContext },
        };

        Gfx.AddScreen(Screen);
    }

    public override Vector2 Origin { get; }

    public GfxScreen Screen { get; }

    public override void SetOffset(Vector2 offset)
    {
        Screen.Offset = offset;
    }

    public override void SetWorldViewProjMatrix(Matrix worldViewProj)
    {
        Screen.RenderOptions.WorldViewProj = worldViewProj;
    }
}