using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class BlackLum : MovableActor
{
    public BlackLum(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Resource = actorResource;
        ShouldDraw = true;

        State.SetTo(Fsm_Idle);
    }

    public ActorResource Resource { get; }
    public Vector2 FlySpeed { get; set; }
    public bool ShouldDraw { get; set; } // Unused

    private bool HasCollidedWithPhysical()
    {
        PhysicalType type = Scene.GetPhysicalType(Position);
        return type.IsSolid || type.Value is PhysicalTypeValue.InstaKill or PhysicalTypeValue.MoltenLava;
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Scene.Camera.IsActorFramed(this) || forceDraw)
        {
            AnimatedObject.IsFramed = true;

            if (ShouldDraw)
                animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }
}