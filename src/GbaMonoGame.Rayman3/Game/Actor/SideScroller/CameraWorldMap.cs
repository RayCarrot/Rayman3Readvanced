using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed class CameraWorldMap : CameraActor2D
{
    public CameraWorldMap(Scene2D scene) : base(scene)
    {
        State.SetTo(null);
    }

    public override void SetFirstPosition()
    {
        Scene.Playfield.Camera.Position = new Vector2(((WorldMap)Frame.Current).ScrollX, 0);
    }
}