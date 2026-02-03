using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class BreakableGround : MovableActor
{
    public BreakableGround(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        QuickFinishBodyAttack = (Action)actorResource.FirstActionId == Action.Idle_QuickFinishBodyShotAttack;

        AnimatedObject.ObjPriority = 60;

        // Destroy actor if it's the one in the hub world and we've defeated the boss
        if ((Action)actorResource.FirstActionId == Action.Idle_World &&
            (GameInfo.PersistentInfo.LastCompletedLevel > (int)MapId.BossRockAndLava ||
             GameInfo.PersistentInfo.LastPlayedLevel > (int)MapId.BossRockAndLava))
        {
            ProcessMessage(this, Message.Destroy);
        }

        State.SetTo(_Fsm_Idle);
    }

    public bool QuickFinishBodyAttack { get; }
    public bool IsDestroyed { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Actor_Hit:
                if (State == _Fsm_Idle && ((RaymanBody)param).BodyPartType == RaymanBody.RaymanBodyPartType.Torso)
                    ActionId = Action.Destroyed;
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Rom.Platform == Platform.NGage && IsDestroyed)
            return;

        DrawLarge(animationPlayer, forceDraw);
    }
}