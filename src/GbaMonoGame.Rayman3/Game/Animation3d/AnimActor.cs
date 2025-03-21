using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GbaMonoGame.Rayman3;

public class AnimActor
{
    public AnimActor(AnimActorResource resource, MeshScreenRenderer meshScreenRenderer)
    {
        Resource = resource;
        MeshScreenRenderer = meshScreenRenderer;

        Animation = null;
        CurrentFrame = 0;
        CurrentAnimTime = 0;
        AnimSpeed = 1;
    }

    public AnimActorResource Resource { get; }
    public MeshScreenRenderer MeshScreenRenderer { get; }

    public Animation3D Animation { get; set; }
    public int CurrentFrame { get; set; }
    public float CurrentAnimTime { get; set; }
    public float AnimSpeed { get; set; }

    public void SetAnimation(int animationId)
    {
        Animation = Resource.Animations[animationId];
        CurrentFrame = 0;
        CurrentAnimTime = 0;
    }

    public void Animate(float deltaTime)
    {
        // NOTE: The game uses the duration value from the animation, but this gives us a more precise float value
        float duration = Animation.FramesCount / (float)Animation.FrameRate;

        CurrentAnimTime += deltaTime * AnimSpeed;

        while (CurrentAnimTime < 0)
            CurrentAnimTime += duration;

        while (duration < CurrentAnimTime)
            CurrentAnimTime -= duration;

        float currentFrameValue = Animation.FrameRate * CurrentAnimTime;
        CurrentFrame = (int)currentFrameValue;

        // NOTE: The game doesn't interpolate the values, but since we have to slow the animation down to simulate
        //       the original lag then this helps makes it smoother.
        AnimationFrame currentAnimFrame = Animation.Frames[CurrentFrame];
        AnimationFrame nextAnimFrame = Animation.Frames[(CurrentFrame + 1) % Animation.FramesCount];
        float rotZ = MathHelper.Lerp(
            currentAnimFrame.RotationZ,
            // Hacky wrapping fix
            nextAnimFrame.RotationZ > currentAnimFrame.RotationZ 
                ? nextAnimFrame.RotationZ - 256 
                : nextAnimFrame.RotationZ,
            currentFrameValue % 1);

        // NOTE: The game sets all rotation values, but only the Z one is actually used. And since we only have a
        //       single rotation matrix it's easiest to just hard-code the x and y values here.
        MeshScreenRenderer.Rotation = new Vector3(
            x: 0,
            y: MathHelpers.Angle256ToRadians(190),
            z: MathHelpers.Angle256ToRadians(rotZ));
    }
}