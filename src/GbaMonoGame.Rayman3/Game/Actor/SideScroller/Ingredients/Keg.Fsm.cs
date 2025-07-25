﻿using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Keg
{
    public bool Fsm_WaitingToFall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                InitialPos = Position;
                ActionId = Action.WaitToFall;
                ShouldDraw = false;
                SpawnedDebrisCount = 0;
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                Box actionBox = GetActionBox();
                actionBox.Bottom += 100;

                // Spawn debris
                if (Timer >= 30 && SpawnedDebrisCount < 2 && Scene.MainActor.GetDetectionBox().Intersects(actionBox))
                {
                    SpawnedDebrisCount++;
                    SpawnDebris();

                    if (Timer > 90)
                        Timer = 30;
                }

                // Fall
                if (Timer > 90 && Scene.MainActor.GetDetectionBox().Intersects(actionBox) && SpawnedDebrisCount > 0)
                {
                    ShouldDraw = true;
                    State.MoveTo(Fsm_Falling);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Falling(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Fall;
                Position = InitialPos;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BarlFall_Mix04);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BarlFall_Mix04);
                LastActorToPlayFallSound = InstanceId;
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this) ||
                    Scene.GetPhysicalType(new Vector2(Position.X, GetDetectionBox().Bottom)).IsSolid)
                {
                    // Optionally fix a bug where the keg might stop the sound of another keg if another one has spawned
                    if (Engine.ActiveConfig.Tweaks.FixBugs && LastActorToPlayFallSound == InstanceId)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BarlFall_Mix04);

                    if (Scene.IsHitMainActor(this))
                        Scene.MainActor.ReceiveDamage(AttackPoints);

                    SpawnExplosion(false);

                    Position = InitialPos;
                    
                    State.MoveTo(Fsm_WaitingToFall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Timer = 0;
                break;
        }

        return true;
    }

    public bool Fsm_InitBossMachine(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                AnimatedObject.BgPriority = 3;
                ActionId = Action.Respawn;

                if (Rom.Platform == Platform.GBA)
                    Position = new Vector2(63, 86);
                else if (Rom.Platform == Platform.NGage)
                    Position = new Vector2(63, 134);
                else
                    throw new UnsupportedPlatformException();
                break;
            
            case FsmAction.Step:
                Timer++;

                // NOTE: The original code here is probably bugged, cause it checks if the different between the same x-positions
                //       are less than 180, which is always true. The code is even optimized out from the GBA version because of
                //       this. Most likely it's meant to check if the main actor is close to the keg when it respawns, so you
                //       actually see it happening, but then more code needs to be updated, like the timer check and an additional
                //       check for the action so it doesn't keep triggering.
                bool ejectFromDispenser;
                if (Engine.ActiveConfig.Tweaks.FixBugs)
                    ejectFromDispenser = ActionId == Action.Respawn && 
                                         Timer >= 60 && 
                                         Math.Abs(Position.X - Scene.MainActor.Position.X) < 180;
                else
                    ejectFromDispenser = Timer == 60 && 
                                         Math.Abs(Position.X - Position.X) < 180;

                if (ejectFromDispenser)
                {
                    ActionId = Action.EjectFromDispenser;
                }
                else if (ActionId == Action.EjectFromDispenser && Speed.Y == 0)
                {
                    if (AnimatedObject.IsFramed)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodImp_Mix03);

                    ActionId = Action.Bounce;
                    Timer = 0;
                }

                if (Timer != 0 && ActionId == Action.Bounce && Speed.Y == 0)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (AnimatedObject.IsFramed)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodImp_Mix03);
                AnimatedObject.BgPriority = 1;
                break;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this) && ((Rayman)Scene.MainActor).AttachedObject == null)
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_PickUpObject, this);

                if (Scene.IsDetectedMainActor(this) && ((Rayman)Scene.MainActor).AttachedObject == this)
                {
                    State.MoveTo(Fsm_PickedUp);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_PickedUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Drop(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Drop;
                break;

            case FsmAction.Step:
                bool landed = false;

                if (IsTouchingMap || Speed.Y == 0)
                {
                    SpawnExplosion(true);
                    landed = true;
                }

                if (landed && GameInfo.MapId == MapId.BossMachine)
                {
                    State.MoveTo(Fsm_InitBossMachine);
                    return false;
                }

                if (landed)
                {
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrownUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.ThrownUp;
                break;

            case FsmAction.Step:
                Box detectionBox = GetDetectionBox();
                Vector2 mapPos = new(Position.X, detectionBox.Bottom);

                if (Scene.IsDetectedMainActor(this) &&
                    ((Rayman)Scene.MainActor).AttachedObject == null &&
                    Speed.Y > 0)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_CatchObject, this);
                }

                bool landed = IsTouchingMap || Scene.GetPhysicalType(mapPos).IsSolid;

                if (Scene.IsHitActor(this) is { } hitObj)
                {
                    if ((ActorType)hitObj.Type is 
                        ActorType.Machine or 
                        ActorType.Cage)
                    {
                        hitObj.ProcessMessage(this, Message.Actor_Hurt, this);
                        landed = true;
                    }
                    else if ((ActorType)hitObj.Type is 
                             ActorType.RedPirate or 
                             ActorType.SilverPirate or 
                             ActorType.BluePirate or 
                             ActorType.GreenPirate or 
                             ActorType.HelicopterBomb or 
                             ActorType.RotatedHelicopterBomb)
                    {
                        hitObj.ReceiveDamage(50);
                        hitObj.ProcessMessage(this, Message.Actor_Hit, this);
                        landed = true;
                    }
                }

                if (landed)
                    SpawnExplosion(true);

                if (Scene.IsDetectedMainActor(this) && ((Rayman)Scene.MainActor).AttachedObject == this && Speed.Y > 0)
                {
                    State.MoveTo(Fsm_PickedUp);
                    return false;
                }

                if (landed && GameInfo.MapId == MapId.BossMachine)
                {
                    State.MoveTo(Fsm_InitBossMachine);
                    return false;
                }

                if (landed)
                {
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrownForward(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Scene.MainActor.IsFacingRight ? Action.ThrownForward_Right : Action.ThrownForward_Left;
                break;

            case FsmAction.Step:
                bool landed = false;
                if (Scene.IsHitActor(this) is { } hitObj)
                {
                    if ((ActorType)hitObj.Type is 
                        ActorType.BreakableDoor or 
                        ActorType.Machine or 
                        ActorType.Cage)
                    {
                        hitObj.ProcessMessage(this, Message.Actor_Hurt);
                        landed = true;
                    }
                    else if ((ActorType)hitObj.Type is 
                             ActorType.RedPirate or 
                             ActorType.SilverPirate or 
                             ActorType.BluePirate or 
                             ActorType.GreenPirate or 
                             ActorType.HelicopterBomb or 
                             ActorType.RotatedHelicopterBomb)
                    {
                        hitObj.ReceiveDamage(50);
                        hitObj.ProcessMessage(this, Message.Actor_Hit, this);
                        landed = true;
                    }
                }

                if (IsTouchingMap)
                    landed = true;

                if (landed)
                    SpawnExplosion(true);

                if (landed && GameInfo.MapId == MapId.BossMachine)
                {
                    State.MoveTo(Fsm_InitBossMachine);
                    return false;
                }

                if (landed)
                {
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Respawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Position = InitialPos;
                Timer = 0;
                ActionId = Action.Respawn;
                break;

            case FsmAction.Step:
                Position = InitialPos;
                Timer++;

                if (ActionId != Action.Idle && Timer > 180)
                {
                    // Respawn if no linked objects, or a linked object is alive
                    bool shouldRespawn = true;
                    foreach (byte? link in Links)
                    {
                        if (link == null)
                            break;

                        shouldRespawn = false;

                        if (Scene.GetGameObject(link.Value).IsEnabled)
                        {
                            shouldRespawn = true;
                            break;
                        }
                    }

                    if (shouldRespawn)
                        ActionId = Action.Idle;
                    else
                        ProcessMessage(this, Message.Destroy);
                }
                else if (ActionId == Action.Idle && Timer == 182 && AnimatedObject.IsFramed)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Appear_SocleFX1_Mix01);
                }

                if (Timer > 240)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Fly(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                CheckAgainstMapCollision = true;
                break;

            case FsmAction.Step:
                bool endFlight = false;
                bool respawn = false;

                Timer++;

                // Start flying after igniting for 1 second
                if (Timer > 60 && ActionId is Action.Ignite_Right or Action.Ignite_Left)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Combust1_Mix02);
                    ActionId = IsFacingRight ? Action.Flying_Right : Action.Flying_Left;
                    Scene.MainActor.ProcessMessage(this, IsFacingRight ? Message.Rayman_FlyWithKegRight : Message.Rayman_FlyWithKegLeft);
                    SpawnedDebrisCount = 0;
                }
                // Flying for 10.5 seconds
                else if (Timer > 630 && ActionId is Action.Flying_Right or Action.Flying_Left)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Combust1_Mix02);
                    ActionId = IsFacingRight ? Action.StopFlying_Right : Action.StopFlying_Left;
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_EndFlyWithKeg);
                }
                // If flying...
                else if (ActionId is Action.Flying_Right or Action.Flying_Left or Action.StopFlying_Right or Action.StopFlying_Left)
                {
                    // End flight if it's been over 12 seconds or no longer attached to the main actor
                    if (Timer > 720 || ((Rayman)Scene.MainActor).AttachedObject != this)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Drop);
                        endFlight = true;
                    }
                    // If the main actor touches the map or is dead
                    else if ((Scene.MainActor.IsTouchingMap && Timer > 75) || Scene.MainActor.HitPoints == 0)
                    {
                        SpawnExplosion(true);
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Explode);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Combust1_Mix02);
                        respawn = true;
                    }
                }

                if (respawn)
                {
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }

                if (endFlight)
                {
                    State.MoveTo(Fsm_FallFromFlight);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_FallFromFlight(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.FallFromFlight_Right : Action.FallFromFlight_Left;
                break;

            case FsmAction.Step:
                bool respawn = false;

                InteractableActor hitPirate = Scene.IsHitActorOfType(this, (int)ActorType.SilverPirate);
                if (hitPirate != null)
                {
                    hitPirate.ReceiveDamage(50);
                    hitPirate.ProcessMessage(this, Message.Actor_Hit, this);
                    respawn = true;
                }

                if (ScreenPosition.X > Scene.Resolution.X + 1 || 
                    ScreenPosition.X < 0 || 
                    Speed.X == 0 || 
                    Scene.GetPhysicalType(Position) == PhysicalTypeValue.InstaKill || 
                    Scene.GetPhysicalType(Position) == PhysicalTypeValue.MoltenLava || 
                    respawn)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Combust1_Mix02);
                    respawn = true;
                    SpawnExplosion(true);
                }

                if (respawn)
                {
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}