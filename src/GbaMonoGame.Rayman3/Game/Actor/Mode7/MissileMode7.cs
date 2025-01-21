using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

// TODO: Implement
public sealed partial class MissileMode7 : Mode7Actor
{
    public MissileMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        // NOTE: Temp code for testing
        AnimatedObject.CurrentAnimation = 25;
        State.SetTo(x =>
        {
            var cam = (TgxCameraMode7)scene.Playfield.Camera;

            if (JoyPad.IsButtonPressed(GbaInput.Up))
                cam.Position += new Vector2(0, 1);
            if (JoyPad.IsButtonPressed(GbaInput.Down))
                cam.Position -= new Vector2(0, 1);
            if (JoyPad.IsButtonPressed(GbaInput.Left))
                cam.Direction++;
            if (JoyPad.IsButtonPressed(GbaInput.Right))
                cam.Direction--;

            cam.Update();

            return false;
        });
    }
}