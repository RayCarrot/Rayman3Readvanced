using GbaMonoGame.AnimEngine;
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

    public override bool IsDebugBoxFramed(AObject obj, Vector2 position)
    {
        TgxCamera cam = Scene.Playfield.Camera;

        obj.ScreenPos = position - cam.Position;

        // Could optimize by only returning true if in view, but not necessary since it's just for debugging
        return true;
    }
}