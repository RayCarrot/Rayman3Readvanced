using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

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
                    break;

                case FsmAction.UnInit:
                    // Do nothing
                    break;
            }

            return false;
        });
    }

    // TODO: Fix this
    public override bool IsActorFramed(BaseActor actor)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        Vector4 worldPos4 = new Vector4(actor.Position, 0, 1);
        Vector4 screenPos4 = Vector4.Transform(worldPos4, cam.BasicEffectShader.View * cam.BasicEffectShader.Projection);
        Vector2 screenPos = new Vector2(screenPos4.X, screenPos4.Y) / screenPos4.W;

        Vector2 pixelPos = (new Vector2(screenPos.X, -screenPos.Y) + Vector2.One) / 2 * actor.AnimatedObject.RenderContext.Resolution;
        actor.AnimatedObject.ScreenPos = pixelPos;

        float distance = screenPos4.W;
        float desiredSize = 100f;
        float scale = desiredSize / distance;

        // Apply the scale to the animated object
        actor.AnimatedObject.AffineMatrix = new AffineMatrix(0, new Vector2(scale));

        //actor.AnimatedObject.ZPriority = distance;

        if (pixelPos.X >= 0 && 
            pixelPos.Y >= 0 && 
            pixelPos.X < actor.AnimatedObject.RenderContext.Resolution.X && 
            pixelPos.Y < actor.AnimatedObject.RenderContext.Resolution.Y)
        {
            return true;
        }
        else
        {
            return false;
        }

        return true;

        //BasicEffect effect = new BasicEffect(Engine.GraphicsDevice);
        ////effect.TextureEnabled = true;
        //effect.VertexColorEnabled = true;
        //effect.Projection = cam.Effect.Projection;
        //effect.View = cam.Effect.View;

        //float dir = cam.Direction + 128;

        //Vector3 cameraOffset = new(
        //    x: MathHelpers.Cos256(dir) * cam.CameraDistance,
        //    y: MathHelpers.Sin256(dir) * cam.CameraDistance,
        //    z: -cam.CameraHeight);

        //Vector3 cameraPosition = new Vector3(cam.Position, 0) + cameraOffset;
        //Vector3 cameraUp = new(0, 0, -1);

        //Vector3 actorPos = new Vector3(actor.Position, 0);

        //Matrix matrix = Matrix.CreateBillboard(actorPos, cameraPosition, cameraUp, null);
        //Matrix lookAt = Matrix.CreateScale(0.2f, -0.2f, 0.2f) * matrix;
        //effect.World = lookAt;

        //actor.AnimatedObject.ScreenPos = Vector2.Zero;
        //actor.AnimatedObject.Shader = effect;

        return true;
    }
}