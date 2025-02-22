using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public abstract class CameraActorMode7 : CameraActor
{
    protected CameraActorMode7(Scene2D scene) : base(scene) { }

    public override bool IsActorFramed(BaseActor actor)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        // Get the camera position and direction
        Vector2 camPos = cam.Position;
        Vector2 camDir = cam.Direction.ToDirectionalVector();

        // Get the difference between the actor and the camera
        Vector2 posDiff = actor.Position - camPos;

        // Check if the actor is in front of the camera
        if (Vector2.Dot(camDir, posDiff) < 0)
            return false;

        // Calculate the actor distance to the camera
        float camDist = posDiff.Length();

        // Check the distance from the camera
        if (camDist >= cam.CameraFar)
            return false;

        // TODO: Add this as an option, enabled by default for modern mode
        // The game doesn't do this, but it looks nicer if we fade in the objects as they enter the view
        const float fadeDist = 40f;
        actor.AnimatedObject.RenderOptions.BlendMode = BlendMode.AlphaBlend;
        actor.AnimatedObject.Alpha = MathF.Min((cam.CameraFar - camDist) / fadeDist, 1);

        // Set the angle relative to the camera
        if (actor is Mode7Actor mode7Actor)
            mode7Actor.CamAngle = MathHelpers.Atan2_256(posDiff);

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
        Vector3 actorPos = new(actor.Position, 0);
        Matrix translationMatrix = Matrix.CreateTranslation(actorPos);

        // Set the world matrix
        Matrix world = scaleMatrix * rotationMatrix * translationMatrix;

        // Get the z position
        float zPos = 0;
        if (actor is Mode7Actor mode7Actor2)
            zPos = mode7Actor2.ZPos / 2;

        // The screen position is 0 since we have the positional data in the world matrix
        actor.AnimatedObject.ScreenPos = new Vector2(0, -zPos);

        // Set the WorldViewProj matrix
        actor.AnimatedObject.RenderOptions.WorldViewProj = world * view * projection;

        // Set the Y priority, used for sorting the objects based on distance
        actor.AnimatedObject.YPriority = camDist;

        return true;
    }

    public override bool IsDebugBoxFramed(AObject obj, Vector2 position)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        Matrix projection = cam.ProjectionMatrix;
        Matrix view = cam.ViewMatrix;
        Matrix world = Matrix.CreateTranslation(new Vector3(position, 0));

        obj.ScreenPos = Vector2.Zero;
        obj.RenderOptions.WorldViewProj = world * view * projection;

        // TODO: Optimize by only returning true if in view
        return true;
    }
}