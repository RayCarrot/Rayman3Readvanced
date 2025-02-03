using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Engine2d;

public abstract class CameraActor2D : CameraActor
{
    protected CameraActor2D(Scene2D scene) : base(scene) { }

    public override bool IsActorFramed(BaseActor actor)
    {
        Box viewBox = actor.GetViewBox();
        Box camBox = new(Scene.Playfield.Camera.Position, Scene.Resolution);

        bool isFramed = viewBox.Intersects(camBox);

        if (isFramed)
            actor.ScreenPosition = actor.Position - Scene.Playfield.Camera.Position;

        return isFramed;
    }

    public override bool IsDebugBoxFramed(DebugBoxAObject obj, Box box)
    {
        TgxCamera cam = Scene.Playfield.Camera;

        obj.Position = box.Position - cam.Position;
        obj.Size = box.Size;

        // TODO: Optimize by only returning true if in view
        return true;
    }
}