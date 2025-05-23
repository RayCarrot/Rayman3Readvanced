﻿using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Depart : ActionActor
{
    public Depart(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        // The behavior to end a level with the sign facing to the right is unused in the final version of the game
        MessageToSend = (Action)actorResource.FirstActionId == Action.EndLevel ? Message.Rayman_FinishLevel : Message.Rayman_ExitLevel;
        State.SetTo(Fsm_Idle);
    }

    public Message MessageToSend { get; }
}