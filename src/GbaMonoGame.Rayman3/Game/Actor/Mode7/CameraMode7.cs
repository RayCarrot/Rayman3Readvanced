using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class CameraMode7 : CameraActorMode7
{
    public CameraMode7(Scene2D scene) : base(scene)
    {
        MainActorDistance = 55;
    }

    public float MainActorDistance { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // TODO: Implement
        return base.ProcessMessageImpl(sender, message, param);
    }

    public override void Step()
    {
        base.Step();

        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;
        cam.Step();
    }

    public override void SetFirstPosition()
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        Mode7Actor linkedActor = (Mode7Actor)LinkedObject;

        // Set the direction as the inverse of the direction of the linked actor
        cam.Direction = -linkedActor.Direction;

        // TODO: Update text layers

        // Set the camera position
        cam.Position = new Vector2(
            x: linkedActor.Position.X - MathHelpers.Cos256(0x100 - cam.Direction) * (MainActorDistance + 90),
            y: linkedActor.Position.Y + MathHelpers.Sin256(0x100 - cam.Direction) * (MainActorDistance + 90));

        State.SetTo(FUN_0801f14c);
    }
}