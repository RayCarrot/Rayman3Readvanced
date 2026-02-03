using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Ammo : BaseActor
{
    public Ammo(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Timer = 0xCD; // Uninitialized, so starts at 0xCD (default allocation value)

        State.SetTo(_Fsm_Default);

        InitialYPosition = Position.Y;
    }

    public float InitialYPosition { get; } // Unused
    public byte Timer { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Timer == 120 && (Scene.Camera.IsActorFramed(this) || forceDraw))
        {
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }
}