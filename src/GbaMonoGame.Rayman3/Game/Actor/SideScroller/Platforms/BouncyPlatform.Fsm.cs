﻿using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class BouncyPlatform
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (!HasTrap)
                    ActionId = Action.Idle;

                if (Rom.Platform == Platform.NGage)
                {
                    Array.Clear(DetectedActors);
                    TriggeredActor = null;
                    MultiplayerCooldown = 20;
                }
                break;

            case FsmAction.Step:
                Timer++;

                // Manage trap
                if (ActionId == Action.Trap && Timer > 180)
                {
                    ActionId = Action.EndTrap;
                }
                else if (IsActionFinished && ActionId == Action.BeginTrap)
                {
                    ActionId = Action.Trap;
                    ChangeAction();
                    Timer = 0;
                }
                else if (IsActionFinished && ActionId == Action.EndTrap)
                {
                    ActionId = Action.Idle;
                }
                else if (ActionId == Action.Trap && Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt, this);
                    ActionId = Action.EndTrap;
                }

                if (Rom.Platform == Platform.NGage && MultiplayerCooldown != 0)
                    MultiplayerCooldown--;

                bool detectedMainActor = false;
                if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive)
                {
                    if (MultiplayerCooldown == 0 && ActionId == Action.Idle)
                    {
                        Box box = GetActionBox();
                        for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                        {
                            MovableActor actor = Scene.GetGameObject<MovableActor>(i);

                            if (actor.GetDetectionBox().Intersects(box))
                            {
                                DetectedActors[i] = actor;
                                TriggeredActor = actor;
                                detectedMainActor = true;
                            }
                        }
                    }
                }
                else
                {
                    detectedMainActor = Scene.IsDetectedMainActor(this);
                }

                if (detectedMainActor && ActionId == Action.Idle)
                {
                    State.MoveTo(Fsm_Bounce);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Bounce(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Bounce00_Mix03);

                if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive)
                {
                    InitialMainActorSpeed = TriggeredActor.Speed;
                    foreach (MovableActor actor in DetectedActors)
                        actor?.ProcessMessage(this, Message.Rayman_BeginBounce, this);
                }
                else
                {
                    InitialMainActorSpeed = Scene.MainActor.Speed;
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginBounce);
                }

                HasTriggeredBounce = false;
                ActionId = Action.Bounce;
                break;

            case FsmAction.Step:
                switch (AnimatedObject.CurrentFrame)
                {
                    case 0:
                        SetDetectionBox(new Box(
                            left: ActorModel.DetectionBox.Left, 
                            top: ActorModel.DetectionBox.Bottom - 20, 
                            right: ActorModel.DetectionBox.Right, 
                            bottom: ActorModel.DetectionBox.Bottom));
                        break;

                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        SetDetectionBox(new Box(
                            left: ActorModel.DetectionBox.Left,
                            top: ActorModel.DetectionBox.Bottom - 14,
                            right: ActorModel.DetectionBox.Right,
                            bottom: ActorModel.DetectionBox.Bottom));
                        break;

                    case 6:
                    case 7:
                        SetDetectionBox(new Box(
                            left: ActorModel.DetectionBox.Left,
                            top: ActorModel.DetectionBox.Bottom - 52,
                            right: ActorModel.DetectionBox.Right,
                            bottom: ActorModel.DetectionBox.Bottom));
                        break;

                    default:
                        SetDetectionBox(new Box(ActorModel.DetectionBox));
                        break;
                }

                if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive)
                {
                    Box box = GetActionBox();
                    for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                    {
                        if (DetectedActors[i] == null)
                        {
                            MovableActor actor = Scene.GetGameObject<MovableActor>(i);

                            if (actor.GetDetectionBox().Intersects(box))
                            {
                                DetectedActors[i] = actor;
                                actor.ProcessMessage(this, Message.Rayman_BeginBounce, this);
                            }
                        }
                    }
                }

                if (AnimatedObject.CurrentFrame > 3 && !HasTriggeredBounce)
                {
                    if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive)
                    {
                        foreach (MovableActor actor in DetectedActors)
                            actor?.ProcessMessage(this, Message.Rayman_Bounce);
                    }
                    else
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_Bounce);
                    }

                    HasTriggeredBounce = true;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SetDetectionBox(new Box(ActorModel.DetectionBox));

                if (HasTrap)
                    ActionId = Action.BeginTrap;
                break;
        }

        return true;
    }
}