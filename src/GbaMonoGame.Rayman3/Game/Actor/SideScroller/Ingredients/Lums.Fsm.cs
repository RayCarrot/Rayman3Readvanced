using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Lums
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (GameInfo.MapId == MapId.BossRockAndLava)
                    Timer = 0;
                break;

            case FsmAction.Step:
                bool collected = false;
                
                if (Scene.MainActor.GetDetectionBox().Intersects(GetViewBox()))
                {
                    if (ActionId == Action.BlueLum)
                        collected = CheckCollision();
                    else
                        collected = CheckCollisionAndAttract(Scene.MainActor.GetDetectionBox());
                }

                if (GameInfo.MapId == MapId.BossRockAndLava && !collected)
                {
                    Timer++;
                    
                    if (Timer > 120)
                    {
                        collected = true;
                        Timer = 1;
                    }
                }

                // Lums have 3 random animations they cycle between, showing different sparkles
                if (IsActionFinished)
                {
                    if (ActionId == Action.BlueLum)
                    {
                        AnimatedObject.CurrentAnimation = 0 + Random.GetNumber(3);
                    }
                    else if (ActionId == Action.WhiteLum || IsGhost)
                    {
                        AnimatedObject.CurrentAnimation = 3 + Random.GetNumber(3);
                    }
                    else if (ActionId is not (Action.BigYellowLum or Action.BigBlueLum))
                    {
                        AnimatedObject.CurrentAnimation = (byte)ActionId * 3 + Random.GetNumber(3);
                    }
                }

                if (collected)
                {
                    State.MoveTo(Fsm_Collected);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Collected(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (GameInfo.MapId == MapId.BossRockAndLava)
                {
                    // Check if the timer finished and the lum should just despawn
                    if (Timer == 1)
                        return true;

                    Timer = 0;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumBleu_Mix02);
                }
                else
                {
                    switch (ActionId)
                    {
                        case Action.YellowLum:
                            if (!IsGhost)
                                GameInfo.KillLum(LumId);
                            
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumOrag_Mix06);
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumOrag_Mix06);
                            
                            // Set a different sound pitch if already collected
                            if (IsGhost)
                                SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__LumOrag_Mix06, 192);
                            break;

                        case Action.RedLum:
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumRed_Mix03);
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumRed_Mix03);
                            break;
                        
                        case Action.GreenLum:
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumGreen_Mix04);
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumGreen_Mix04);

                            Vector2 pos = Position;
                            pos.Y -= MathHelpers.Mod(pos.Y, Tile.Size);
                            while (Scene.GetPhysicalType(pos) == PhysicalTypeValue.None)
                                pos += new Vector2(0, Tile.Size);

                            GameInfo.GreenLumTouchedByRayman(LumId, pos);
                            break;

                        case Action.BlueLum:
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumBleu_Mix02);
                            break;

                        case Action.WhiteLum:
                            GameInfo.HasCollectedWhiteLum = true;
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumSlvr_Mix02);
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumSlvr_Mix02);
                            break;

                        case Action.BigYellowLum:
                            // Do nothing
                            break;

                        case Action.BigBlueLum:
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumBleu_Mix02);
                            break;
                    }
                }

                if (!IsGhost)
                    Scene.MainActor.ProcessMessage(this, ActionId switch
                    {
                        Action.YellowLum => Message.Rayman_CollectYellowLum,
                        Action.RedLum => Message.Rayman_CollectRedLum,
                        Action.GreenLum => Message.Rayman_CollectGreenLum,
                        Action.BlueLum => Message.Rayman_CollectBlueLum,
                        Action.WhiteLum => Message.Rayman_CollectWhiteLum,
                        Action.BigYellowLum => Message.Rayman_CollectBigYellowLum,
                        Action.BigBlueLum => Message.Rayman_CollectBigBlueLum,
                        _ => throw new ArgumentOutOfRangeException(nameof(ActionId), ActionId, null)
                    });
                AnimatedObject.CurrentAnimation = 9;
                break;

            case FsmAction.Step:
                if (IsActionFinished && ActionId == Action.BlueLum && GameInfo.MapId != MapId.BossRockAndLava)
                {
                    State.MoveTo(Fsm_Delay);
                    return false;
                }
                
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (ActionId != Action.BlueLum || GameInfo.MapId == MapId.BossRockAndLava)
                    ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }

    public bool Fsm_Delay(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                if (Timer >= 250)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Vector2 camPos = Scene.Playfield.Camera.Position;

                if (Position.X - camPos.X > 0 &&
                    Position.X - camPos.X < Scene.Resolution.X &&
                    Position.Y - camPos.Y > 0 &&
                    Position.Y - camPos.Y < Scene.Resolution.Y)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Appear_SocleFX1_Mix01);
                }

                if (ActionId == Action.BlueLum)
                    AnimatedObject.CurrentAnimation = 0;

                Timer = 0xFF;
                break;
        }

        return true;
    }

    public bool Fsm_MultiplayerIdle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                bool collected = false;

                if (Timer == 0xFF)
                {
                    Box viewBox = GetViewBox();
                    int currentFramePlayerId = (int)(MultiplayerManager.GetElapsedTime() % RSMultiplayer.MaxPlayersCount);

                    if (currentFramePlayerId < MultiplayerManager.PlayersCount)
                    {
                        MovableActor player = Scene.GetGameObject<MovableActor>(currentFramePlayerId);

                        if (player.GetDetectionBox().Intersects(viewBox))
                        {
                            Timer = (byte)currentFramePlayerId;
                            collected = CheckCollisionAndAttract(player.GetDetectionBox());
                        }

                    }
                }
                else
                {
                    Box viewBox = GetViewBox();
                    MovableActor player = Scene.GetGameObject<MovableActor>(Timer);

                    if (player.GetDetectionBox().Intersects(viewBox))
                        collected = CheckCollisionAndAttract(player.GetDetectionBox());
                    else
                        Timer = 0xFF;
                }

                if (IsActionFinished && ActionId == Action.BlueLum)
                    AnimatedObject.CurrentAnimation = Random.GetNumber(3);

                if (collected)
                {
                    State.MoveTo(Fsm_MultiplayerCollected);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MultiplayerCollected(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (ActionId == Action.BlueLum)
                {
                    if (Timer == MultiplayerManager.MachineId)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumBleu_Mix02);
                    }
                }
                else if (ActionId == Action.WhiteLum)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumSlvr_Mix02);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumSlvr_Mix02);
                }

                Scene.GetGameObject(Timer).ProcessMessage(this, ActionId switch
                {
                    Action.YellowLum => Message.Rayman_CollectYellowLum,
                    Action.RedLum => Message.Rayman_CollectRedLum,
                    Action.GreenLum => Message.Rayman_CollectGreenLum,
                    Action.BlueLum => Message.Rayman_CollectBlueLum,
                    Action.WhiteLum => Message.Rayman_CollectWhiteLum,
                    Action.BigYellowLum => Message.Rayman_CollectBigYellowLum,
                    Action.BigBlueLum => Message.Rayman_CollectBigBlueLum,
                    _ => throw new ArgumentOutOfRangeException(nameof(ActionId), ActionId, null)
                });
                AnimatedObject.CurrentAnimation = 9;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_MultiplayerDelay);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MultiplayerDelay(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                if (ActionId is Action.BlueLum or Action.BigBlueLum && Timer >= 250)
                {
                    State.MoveTo(Fsm_MultiplayerIdle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Vector2 pos = MultiplayerInfo.TagInfo.GetLumPosition(LumId);

                Debug.Assert(pos != Vector2.Zero);

                Position = pos;

                // NOTE: The game also checks for big blue lum state here, but the actual animation code doesn't account for it
                if (ActionId == Action.BlueLum)
                    AnimatedObject.CurrentAnimation = 0;

                Timer = 0xFF;
                break;
        }

        return true;
    }
}