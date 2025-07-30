using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public abstract class CameraActorMode7 : CameraActor
{
    protected CameraActorMode7(Scene2D scene) : base(scene) { }

    public float FadeDistance { get; set; } = 40;

    // NOTE: Currently we ignore if the object should use affine rendering or not and always render it in 3D. In the
    //       original game this would determine if the object should be scaled based on the camera distance. Usually
    //       the reason for this being disabled was due to rendering limitations on the GBA, but it does now make the
    //       sprites render at a different size for us than in the original game.

    // Custom method so we can use IsActorFramed without an actor
    public bool IsAnimatedObjectFramed(AnimatedObject animatedObject, Vector2 position, float zPos, bool isAffine)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        // Get the camera position and direction
        Vector2 camPos = cam.Position;
        Vector2 camDir = cam.Direction.ToDirectionalVector();

        // Get the difference between the object and the camera
        Vector2 posDiff = position - camPos;

        // Check if the object is in front of the camera
        if (Vector2.Dot(camDir, posDiff) < 0)
            return false;

        // Calculate the object distance to the camera
        float camDist = posDiff.Length();

        // Check the distance from the camera
        if (camDist >= cam.CameraFar)
            return false;

        if (Engine.ActiveConfig.Tweaks.VisualImprovements)
        {
            // The game doesn't do this, but it looks nicer if we fade in the objects as they enter the view
            animatedObject.RenderOptions.BlendMode = BlendMode.AlphaBlend;
            animatedObject.Alpha = MathF.Min((cam.CameraFar - camDist) / FadeDistance, AlphaCoefficient.MaxValue);
        }
        else
        {
            animatedObject.RenderOptions.BlendMode = BlendMode.None;
            animatedObject.Alpha = AlphaCoefficient.MaxValue;
        }

        // Get the projection and view from the camera
        Matrix projection = cam.ProjectionMatrix;
        Matrix view = cam.ViewMatrix;

        // Get the scale
        const float scale = 0.25f;
        Matrix scaleMatrix = Matrix.CreateScale(scale, -scale, scale);

        // Get the rotation to face the camera
        view.Decompose(out _, out Quaternion rotation, out _);
        Matrix rotationMatrix = Matrix.Invert(Matrix.CreateFromQuaternion(rotation));

        // Get the translation
        Vector3 objPos = new(position, 0);
        Matrix translationMatrix = Matrix.CreateTranslation(objPos);

        // Set the world matrix
        Matrix world = scaleMatrix * rotationMatrix * translationMatrix;

        // The screen position is 0 since we have the positional data in the world matrix
        animatedObject.ScreenPos = new Vector2(0, -zPos / 2);

        // Set the WorldViewProj matrix
        animatedObject.RenderOptions.WorldViewProj = world * view * projection;
        animatedObject.RenderOptions.UseDepthStencil = true;

        // Set the Y priority, used for sorting the objects based on distance
        animatedObject.YPriority = camDist;

        // Set affine flag to match the original game. This is needed to disable the
        // sprite flipping for the Mode7 tree trunks.
        animatedObject.SetFlagUseRotationScaling(isAffine);

        return true;
    }

    public override bool IsActorFramed(BaseActor actor)
    {
        // Get the z position
        float zPos = 0;
        if (actor is Mode7Actor mode7Actor2)
            zPos = mode7Actor2.ZPos;

        bool isFramed = IsAnimatedObjectFramed(actor.AnimatedObject, actor.Position, zPos, actor is Mode7Actor { IsAffine: true });

        if (actor is Mode7Actor mode7Actor)
        {
            TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

            // Get the difference between the actor and the camera
            Vector2 posDiff = actor.Position - cam.Position;

            // Set the angle relative to the camera
            mode7Actor.CamAngle = MathHelpers.Atan2_256(posDiff);
        }

        return isFramed;
    }

    public override bool IsDebugBoxFramed(AObject obj, Vector2 position)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        Matrix projection = cam.ProjectionMatrix;
        Matrix view = cam.ViewMatrix;
        Matrix world = Matrix.CreateTranslation(new Vector3(position, 0));

        obj.ScreenPos = Vector2.Zero;
        obj.RenderOptions.WorldViewProj = world * view * projection;

        // Could optimize by only returning true if in view, but not necessary since it's just for debugging
        return true;
    }
}