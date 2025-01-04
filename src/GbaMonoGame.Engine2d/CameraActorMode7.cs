using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public abstract class CameraActorMode7 : CameraActor
{
    protected CameraActorMode7(Scene2D scene) : base(scene) { }

    public override bool IsActorFramed(BaseActor actor)
    {
        // TODO: Remove this once we implement the actors - this is just temp code to avoid crashing!
        if (actor is not Mode7Actor)
            return false;

        // TODO: A LOT of this has to be rewritten and cleaned up. The Mode7 code is very confusing, and
        //       there's a lot of fixed-point math and pre-calculated lookup tables being used...

        Mode7Actor mode7Actor = (Mode7Actor)actor;
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        bool isFramed = false;
        byte v63 = mode7Actor.field_0x63;
        short v60 = mode7Actor.field_0x60;
        float actorCamAngle = 0;
        float scale = 0;

        Vector2 posDiff = mode7Actor.Position - Scene.Playfield.Camera.Position;
        Vector2 camDirection = new(MathHelpers.Cos256(cam.Direction), MathHelpers.Sin256(cam.Direction));
        
        // Check if the actor is in front of the camera
        if (Vector2.Dot(camDirection, posDiff) >= 0)
        {
            // Calculate the actor distance to the camera
            float camDist = posDiff.Length();

            // Check the distance from the camera
            if (camDist <= cam.MaxDist)
            {
                float angle = MathHelpers.Atan2_256(posDiff);
                actorCamAngle = angle;

                float uVar5 = cam.Direction + cam.field_0xb49;
                float iVar15 = angle - uVar5;
                float uVar11 = 6 - cam.field_0xb49; // What is the 6?

                // What is this check?
                if (2 * uVar11 >= iVar15 || iVar15 >= 243)
                {
                    float iVar13 = uVar11;
                    float uVar12 = angle - cam.Direction;

                    // Some perspective correction?
                    if (uVar12 is > 2 and < 254)
                        iVar13 = MathHelpers.Cos256(uVar12) * 7 * iVar13;

                    // TODO: Determine from pre-calculated list
                    scale = 1;
                    int scaleIndex = 0;

                    if (scale >= 0.5f)
                    {
                        // Huh?
                        actor.ScreenPosition = new Vector2(angle - uVar5 * 0x5b6d, scaleIndex - (((v63 + v60) / scale) / 2 + 2));
                        isFramed = true;
                    }
                }
            }
        }

        if (isFramed)
        {
            if (mode7Actor.IsAffine)
            {
                mode7Actor.AnimatedObject.AffineMatrix = new AffineMatrix(scale, 0, 0, scale);
                mode7Actor.AnimatedObject.IsDoubleAffine = scale >= 1;
            }

            // TODO: Set priority
        }
        else
        {
            actor.ScreenPosition = actor.ScreenPosition with { X = 384 };
        }

        mode7Actor.CamAngle = actorCamAngle;

        return isFramed;
    }
}