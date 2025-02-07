using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GbaMonoGame.Rayman3;

public abstract class CameraActorMode7 : CameraActor
{
    protected CameraActorMode7(Scene2D scene) : base(scene) { }

    public override bool IsActorFramed(BaseActor actor)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        // Get the camera position and direction
        Vector2 camPos = cam.Position;
        Vector2 camDir = cam.GetDirection();

        // Get the difference between the actor and the camera
        Vector2 posDiff = actor.Position - camPos;

        // Check if the actor is in front of the camera
        if (Vector2.Dot(camDir, posDiff) < 0)
            return false;

        // Calculate the actor distance to the camera
        float camDist = posDiff.Length();

        // Check the distance from the camera
        if (camDist > cam.CameraFar)
            return false;

        // Set the angle relative to the camera
        if (actor is Mode7Actor mode7Actor)
            mode7Actor.CamAngle = MathHelpers.Atan2_256(posDiff);

        // Get or create the shader for the actor
        if (!cam.CachedBasicEffectShaders.TryGetValue(actor.AnimatedObject, out BasicEffect effect))
        {
            effect = new BasicEffect(Engine.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = true,
            };

            cam.CachedBasicEffectShaders.Add(actor.AnimatedObject, effect);
        }

        // Set the projection and view from the camera
        effect.Projection = cam.BasicEffectShader.Projection;
        effect.View = cam.BasicEffectShader.View;

        // Get the scale
        const float scale = 0.25f;
        Matrix scaleMatrix = Matrix.CreateScale(scale, -scale, scale);

        // Get the rotation to face the camera
        effect.View.Decompose(out _, out Quaternion rotation, out _);
        Matrix rotationMatrix = Matrix.Invert(Matrix.CreateFromQuaternion(rotation));

        // Get the translation
        Vector3 actorPos = new(actor.Position, 0);
        Matrix translationMatrix = Matrix.CreateTranslation(actorPos);

        // Set the world matrix
        effect.World = scaleMatrix * rotationMatrix * translationMatrix;

        // The screen position is 0 since we use the shader
        actor.AnimatedObject.ScreenPos = Vector2.Zero;

        // Set the shader
        actor.AnimatedObject.Shader = effect;

        // Set the Y priority, used for sorting the objects based on distance
        actor.AnimatedObject.YPriority = camDist;

        return true;
    }

    public override bool IsDebugBoxFramed(AObject obj, Vector2 position)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        if (!cam.CachedBasicEffectShaders.TryGetValue(obj, out BasicEffect effect))
        {
            effect = new BasicEffect(Engine.GraphicsDevice)
            {
                VertexColorEnabled = true
            };

            cam.CachedBasicEffectShaders.Add(obj, effect);
        }

        effect.Projection = cam.BasicEffectShader.Projection;
        effect.View = cam.BasicEffectShader.View;
        effect.World = Matrix.CreateTranslation(new Vector3(position, 0));

        obj.ScreenPos = Vector2.Zero;
        obj.Shader = effect;

        // TODO: Optimize by only returning true if in view
        return true;
    }
}