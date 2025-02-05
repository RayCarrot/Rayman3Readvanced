using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class CameraMode7
{
    // TODO: Implement
    public bool FUN_0801f14c(FsmAction action)
    {
        // NOTE: Temp code for testing - allows freely moving the camera
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

                Vector2 direction = new(MathHelpers.Cos256(cam.Direction), MathHelpers.Sin256(cam.Direction));
                Vector2 sideDirection = new(MathHelpers.Cos256(cam.Direction - 64), MathHelpers.Sin256(cam.Direction - 64));

                const float speed = 1;

                if (JoyPad.IsButtonPressed(GbaInput.Up))
                    cam.Position += direction * speed;
                if (JoyPad.IsButtonPressed(GbaInput.Down))
                    cam.Position -= direction * speed;

                if (JoyPad.IsButtonPressed(GbaInput.Right))
                    cam.Position -= sideDirection * speed;
                if (JoyPad.IsButtonPressed(GbaInput.Left))
                    cam.Position += sideDirection * speed;

                if (JoyPad.IsButtonPressed(GbaInput.R))
                    cam.Direction--;
                if (JoyPad.IsButtonPressed(GbaInput.L))
                    cam.Direction++;

                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return false;
    }
}