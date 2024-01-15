﻿using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class RaymanBody : MovableActor
{
    public RaymanBody(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Rayman = Scene.MainActor;
        AnimatedObject.YPriority = 18;
        Fsm.ChangeAction(Fsm_Wait);
    }

    public MovableActor Rayman { get; set; }
    public RaymanBodyPartType BodyPartType { get; set; }
    public uint ChargePower { get; set; }
    public bool HasCharged { get; set; }
    public byte BaseActionId { get; set; }
    public InteractableActor HitActor { get; set; }

    private void SpawnHitEffect()
    {
        RaymanBody hitEffectActor = Scene.KnotManager.CreateProjectile<RaymanBody>(ActorType.RaymanBody);
        if (hitEffectActor != null)
        {
            hitEffectActor.BodyPartType = RaymanBodyPartType.HitEffect;
            hitEffectActor.Position = Position;
            hitEffectActor.HasMapCollision = false;
            hitEffectActor.ActionId = 25;
            hitEffectActor.AnimatedObject.YPriority = 1;
            hitEffectActor.ChangeAction();
        }
    }

    protected override bool ProcessMessageImpl(Message message, object param)
    {
        if (base.ProcessMessageImpl(message, param))
            return false;

        switch (message)
        {
            case Message.RaymanBody_FinishedAttack:
                throw new NotImplementedException();
                return false;

            default:
                return false;
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