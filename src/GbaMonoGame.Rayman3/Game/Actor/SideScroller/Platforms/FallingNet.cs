﻿using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class FallingNet : MovableActor
{
    public FallingNet(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialPosition = Position;

        State.SetTo(Fsm_Idle);
    }

    public Vector2 InitialPosition { get; }
    public byte Timer { get; set; }

    // Custom
    public bool DisabledFromLink { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return true;

        switch (message)
        {
            case Message.Readvanced_RespawnDeath:
                if (!DisabledFromLink)
                    ProcessMessage(this, Message.Resurrect);
                return true;

            default:
                return false;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        DisabledFromLink = DestroyIfPastLinkedCheckpoint(actorResource);
    }
}