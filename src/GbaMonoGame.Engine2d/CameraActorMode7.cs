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
        byte v63 = mode7Actor.field_0x63; // Height?
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

                // TODO: Is it actually doing mod for all of these?
                float iVar15 = MathHelpers.Mod((angle - uVar5), 256);
                float uVar11 = MathHelpers.Mod(6 - cam.field_0xb49, 256); // What is the 6?
                
                // What is this check?
                if (2 * uVar11 >= iVar15 || iVar15 >= 243)
                {
                    float iVar13 = camDist;
                    float uVar12 = angle - cam.Direction;

                    // Some perspective correction?
                    if (uVar12 is > 2 and < 254)
                        iVar13 = MathHelpers.Cos256(uVar12) * 7 * iVar13;

                    int scaleIndex = FUN_080a3a1c(cam.Scales, (int)iVar13);
                    scale = cam.Scales[scaleIndex] * 4;

                    if (scale >= 0.5f)
                    {
                        // Huh?
                        short x = (short)(((int)(((uint)angle - ((int)uVar5 * 0x100)) * 0x10000) >> 0x10) * 0x5b6d + 0x80000 >> 0x14);
                        actor.ScreenPosition = new Vector2(x, scaleIndex - (((v63 + v60) * 0x100 / scale - v63) / 2 + 2));
                        isFramed = true;
                    }
                }
            }
        }

        if (isFramed)
        {
            if (mode7Actor.IsAffine)
            {
                scale = scale / 256f;
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

    // Binary search
    int FUN_080a3a1c(float[] values, float param_2)
    {
        int iVar2 = 128;
        uint uVar3 = 64;
        int iVar4 = 0;
        do
        {
            uint uVar1;
            if (param_2 > values[iVar2])
            {
                uVar1 = (uint)-uVar3;
            }
            else
            {
                uVar1 = uVar3;
            }
            iVar2 = (int)(iVar2 + uVar1);
            uVar3 >>= 1;
            iVar4++;
        } while (iVar4 < 7);

        for (iVar4 = iVar2 - 1; iVar4 < iVar2 + 2 && param_2 <= values[iVar4]; iVar4++)
        {

        }
        return iVar4;
    }
}