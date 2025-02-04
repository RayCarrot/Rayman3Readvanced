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
    protected CameraActorMode7(Scene2D scene) : base(scene)
    {
        // NOTE: Temp code for testing - allows freely moving the camera
        State.SetTo(action =>
        {
            switch (action)
            {
                case FsmAction.Init:
                    // Do nothing
                    break;

                case FsmAction.Step:
                    TgxCameraMode7 cam = (TgxCameraMode7)scene.Playfield.Camera;

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

                    cam.Step();
                    break;

                case FsmAction.UnInit:
                    // Do nothing
                    break;
            }

            return false;
        });
    }

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

        const float baseScale = 0.5f;

        // TODO: Replace with the RenderHeight value from the actor
        AnimationChannel channel = actor.AnimatedObject.GetAnimation().Channels[0];
        Constants.Size shape = Constants.GetSpriteShape(channel.SpriteShape, channel.SpriteSize);
        float renderHeight = shape.Height;

        // Get the 3D position and offset by half the height so it's above the floor
        Vector3 actorPos = new(actor.Position, -(renderHeight / 2f) * baseScale);

        // Project to the screen
        Vector3 screenPos = cam.Project(actorPos);

        // Set the screen position
        actor.AnimatedObject.ScreenPos = new Vector2(screenPos.X, screenPos.Y);

        // Set the Y priority, used for sorting the objects based on distance
        actor.AnimatedObject.YPriority = screenPos.Z;

        // Get a second screen position one unit away to determine the scale
        Vector3 screenPos2 = cam.Project(actorPos + new Vector3(0, 0, 1));
        float scale = (new Vector2(screenPos.X, screenPos.Y) - new Vector2(screenPos2.X, screenPos2.Y)).Length();
        actor.AnimatedObject.AffineMatrix = new AffineMatrix(0, new Vector2(scale * baseScale));
        
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