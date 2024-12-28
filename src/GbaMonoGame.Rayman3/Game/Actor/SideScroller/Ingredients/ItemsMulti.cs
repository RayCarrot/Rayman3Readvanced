using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class ItemsMulti : BaseActor
{
    public ItemsMulti(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.CurrentAnimation = actorResource.FirstActionId;
        ActionId = (Action)actorResource.FirstActionId;
        
        Timer = 0;

        if (ActionId == Action.Random)
        {
            IsRandomized = true;
            ActionId = Action.Globox;
        }
        else
        {
            IsRandomized = false;
        }

        if (ActionId is Action.Globox or Action.Reverse or Action.Invisibility)
        {
            MultiplayerInfo.TagInfo.RegisterItem(instanceId);
            SpawnCountdown = 0xff;
        }
        else
        {
            SpawnCountdown = 0;
        }

        State.SetTo(Fsm_Default);
    }

    public ushort Timer { get; set; }
    public byte SpawnCountdown { get; set; }
    public bool IsRandomized { get; set; }

    public bool IsInvisibleItem()
    {
        if (IsRandomized)
            return false;
        else
            return ActionId == Action.Invisibility;
    }

    public Action Spawn()
    {
        SpawnCountdown = 120;

        if (IsRandomized)
        {
            ActionId = (Action)MultiplayerInfo.TagInfo.GetRandomActionId();
            AnimatedObject.CurrentAnimation = (int)ActionId;
        }

        return ActionId;
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (SpawnCountdown <= 20 || (SpawnCountdown > 120 && ActionId == Action.Fist && AnimatedObject.ObjPriority == 0))
            base.Draw(animationPlayer, forceDraw);
    }
}