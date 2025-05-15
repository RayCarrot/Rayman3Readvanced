using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class RaymanBody : MovableActor
{
    public RaymanBody(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Rayman = (Rayman)Scene.MainActor;
        AnimatedObject.ObjPriority = 18;

        State.SetTo(Fsm_Wait);
    }

    public Rayman Rayman { get; set; }
    public RaymanBodyPartType BodyPartType { get; set; }
    public uint ChargePower { get; set; }
    public bool HasCharged { get; set; }
    public byte BaseActionId { get; set; }
    public InteractableActor HitActor { get; set; }

    private void SpawnHitEffect()
    {
        RaymanBody hitEffectActor = Scene.CreateProjectile<RaymanBody>(ActorType.RaymanBody);
        if (hitEffectActor != null)
        {
            hitEffectActor.BodyPartType = RaymanBodyPartType.HitEffect;
            hitEffectActor.Position = Position;
            hitEffectActor.CheckAgainstMapCollision = false;
            hitEffectActor.ActionId = Action.HitEffect;
            hitEffectActor.AnimatedObject.ObjPriority = 1;
            hitEffectActor.ChangeAction();
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.RaymanBody_FinishAttack:
                if (State != Fsm_MoveBackwards)
                {
                    if (BodyPartType == RaymanBodyPartType.Torso)
                        ActionId = IsFacingRight ? Action.Torso_MoveBackwards_Right : Action.Torso_MoveBackwards_Left;
                    else
                        ActionId = (Action)(BaseActionId + (IsFacingRight ? 4 : 3));

                    ChangeAction();
                    State.MoveTo(Fsm_MoveBackwards);
                }
                SpawnHitEffect();
                return false;

            default:
                return false;
        }
    }

    // Game overrides this and calls PlayChannelBox even when not on screen. Makes sense since it should
    // retain its collision. But it seems unnecessary since ComputeNextFrame calls that as well...
    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // Copy over Rayman's alpha blending. The original game doesn't do this since the alpha
        // is global, but here we have to since it's managed per object instead.
        AnimatedObject.Alpha = Rayman.AnimatedObject.Alpha;

        if (Scene.Camera.IsActorFramed(this) || forceDraw)
        {
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.PlayChannelBox();
            AnimatedObject.ComputeNextFrame();
        }
    }

    public enum RaymanBodyPartType
    {
        Fist = 0,
        SecondFist = 1,
        Foot = 2,
        Torso = 3,
        HitEffect = 4,
        SuperFist = 5,
        SecondSuperFist = 6,
    }
}