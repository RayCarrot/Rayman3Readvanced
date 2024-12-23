using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

// TODO: Implement
public class TgxCameraMode7 : TgxCamera
{
    public TgxCameraMode7(RenderContext renderContext) : base(renderContext) { }

    public override Vector2 Position { get; set; }

    public int MaxDist { get; set; }
    public byte field_0xb49 { get; set; }
    public byte Direction { get; set; }
}