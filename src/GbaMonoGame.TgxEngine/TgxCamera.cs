using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public abstract class TgxCamera
{
    protected TgxCamera(RenderContext renderContext)
    {
        RenderContext = renderContext;
    }

    public RenderContext RenderContext { get; }
    public abstract Vector2 Position { get; set; }
}