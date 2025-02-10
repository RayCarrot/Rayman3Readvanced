using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class CameraMode7 : CameraActorMode7
{
    public CameraMode7(Scene2D scene) : base(scene)
    {
        IsWaterSki = false;
        //field_0x1c = 0;
        //field_0x1e = 0;
        Timer = 0;
        MainActorDistance = 55;
        DirectionDelta = 0;
    }

    public bool IsWaterSki { get; set; }
    public uint Timer { get; set; }
    public float MainActorDistance { get; set; }
    public float DirectionDelta { get; set; }

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

        // Set the camera position
        Vector2 directionalVector = MathHelpers.DirectionalVector256(0x100 - cam.Direction);
        cam.Position = new Vector2(
            x: linkedActor.Position.X - directionalVector.X * (MainActorDistance + 90),
            y: linkedActor.Position.Y + directionalVector.Y * (MainActorDistance + 90));

        State.SetTo(Fsm_Init);
    }
}