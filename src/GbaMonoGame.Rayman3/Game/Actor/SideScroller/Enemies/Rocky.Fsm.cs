using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public partial class Rocky
{
    // Rayman spawns within 200 pixels of Rocky, so this goes unused
    public bool Fsm_Init(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Sleep_Left;
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);

                // If in time attack then wait for the countdown to finish
                if ((!TimeAttackInfo.IsActive || TimeAttackInfo.Mode == TimeAttackMode.Play) && 
                    Math.Abs(Position.X - Scene.MainActor.Position.X) < 200)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (BossHealth == 1)
                    ActionId = IsFacingRight ? Action.IdlePhase3_Right : Action.IdlePhase3_Left;
                else if (BossHealth == 2)
                    ActionId = IsFacingRight ? Action.IdlePhase2_Right : Action.IdlePhase2_Left;
                else if (BossHealth == 3) 
                    ActionId = IsFacingRight ? Action.IdlePhase1_Right : Action.IdlePhase1_Left;

                Timer = 0;
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);
                Timer++;

                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(1);
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                }

                Rayman rayman = (Rayman)Scene.MainActor;
                if (rayman.State == rayman.Fsm_SuperHelico && ActionId is not (Action.PreparePunch_Right or Action.PreparePunch_Left))
                {
                    ActionId = IsFacingRight ? Action.PreparePunch_Right : Action.PreparePunch_Left;
                }
                else if (rayman.State != rayman.Fsm_SuperHelico && ActionId is Action.PreparePunch_Right or Action.PreparePunch_Left)
                {
                    if (BossHealth == 1)
                        ActionId = IsFacingRight ? Action.IdlePhase3_Right : Action.IdlePhase3_Left;
                    else if (BossHealth == 2)
                        ActionId = IsFacingRight ? Action.IdlePhase2_Right : Action.IdlePhase2_Left;
                    else if (BossHealth == 3)
                        ActionId = IsFacingRight ? Action.IdlePhase1_Right : Action.IdlePhase1_Left;
                }

                if (rayman.State == rayman.Fsm_SuperHelico && 
                    Math.Abs(Position.X - Scene.MainActor.Position.X) < 90)
                {
                    State.MoveTo(Fsm_PunchAttack);
                    return false;
                }

                if (!Scene.MainActor.IsInvulnerable && 
                    rayman.State != rayman.Fsm_SuperHelico && 
                    BlueLum is not { IsEnabled: true } && 
                    Timer > 60)
                {
                    State.MoveTo(Fsm_SlamAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SlamAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Slam_Right : Action.Slam_Left;
                Timer = 0;
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);

                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(1);
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                }

                if (AnimatedObject.CurrentFrame == 9 && Timer == 0)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BigFoot1_Mix02);
                    Scene.Camera.ProcessMessage(this, Message.Cam_Shake, 128);
                    SpawnFlames();
                    Timer = 1;
                }

                Rayman rayman = (Rayman)Scene.MainActor;
                if (IsActionFinished || rayman.State == rayman.Fsm_SuperHelico)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (AttackCount == 0)
                    SpawnBlueLum();
                break;
        }

        return true;
    }

    public bool Fsm_PunchAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossVO02_Mix01);
                ActionId = IsFacingRight ? Action.Punch_Right : Action.Punch_Left;
                Timer = 0;
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);

                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(1);
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                }

                if (ActionId is Action.Punch_Right or Action.Punch_Left)
                    Timer++;

                if (IsActionFinished && Timer != 0)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ChargeAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AttackCount--;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Combust1_Mix02);
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);

                if (Scene.IsHitMainActor(this) && !Scene.MainActor.IsInvulnerable)
                {
                    Scene.MainActor.ReceiveDamage(1);
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_HurtPassthrough);
                }

                if (IsActionFinished && ActionId is Action.PrepareCharge_Right or Action.PrepareCharge_Left)
                    ActionId = IsFacingRight ? Action.Charge_Right : Action.Charge_Left;

                if (AttackCount == 0 && (AnimatedObject.ScreenPos.X < -50 || AnimatedObject.ScreenPos.X > Scene.Resolution.X + 30))
                {
                    State.MoveTo(Fsm_Land);
                    return false;
                }

                if (AttackCount != 0 && (AnimatedObject.ScreenPos.X < -50 || AnimatedObject.ScreenPos.X > Scene.Resolution.X + 30))
                {
                    State.MoveTo(Fsm_ChargeAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (AttackCount == 0)
                {
                    Position = Position with { Y = InitialYPosition };
                }
                else
                {
                    // Change direction
                    if (IsFacingLeft)
                    {
                        AnimatedObject.FlipX = false;
                        Position = new Vector2(0, InitialYPosition);
                    }
                    else
                    {
                        AnimatedObject.FlipX = true;
                        Position = new Vector2(Scene.Resolution.X, 160);
                    }

                    ActionId = IsFacingRight ? Action.Charge_Right : Action.Charge_Left;
                }

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Combust1_Mix02);
                break;
        }

        return true;
    }

    public bool Fsm_Land(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (IsFacingLeft)
                {
                    ActionId = Action.Fall_Right;
                    Position = new Vector2(30, 0);
                }
                else
                {
                    ActionId = Action.Fall_Left;
                    Position = new Vector2(210, 0);
                }

                Timer = 0;
                AttackCount = BossHealth == 3 ? 5 : 8;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Combust1_Mix02);
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);

                if (Scene.IsHitMainActor(this))
                {
                    if (!Engine.ActiveConfig.Difficulty.NoInstaKills)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Explode);
                    }
                    else
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                        Scene.MainActor.ReceiveDamage(2);
                    }
                }

                Timer++;

                if (Timer < 60)
                    Position -= new Vector2(0, 4);

                if (Position.Y > InitialYPosition)
                {
                    Position = Position with { Y = InitialYPosition };
                    ActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    ChangeAction();
                }

                if (IsActionFinished && ActionId is Action.Land_Right or Action.Land_Left)
                {
                    State.MoveTo(Fsm_SlamAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Combust1_Mix02);
                break;
        }

        return true;
    }

    public bool Fsm_Hit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MetlImp1_PiraHit3_Mix03);
                ActionId = IsFacingRight ? Action.Hit_Right : Action.Hit_Left;
                BossHealth--;
                ((FrameSideScroller)Frame.Current).UserInfo.BossHit();
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);
                Scene.MainActor.ProcessMessage(this, Message.Rayman_EndSuperHelico);
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);

                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(1);
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                }

                Rayman rayman = (Rayman)Scene.MainActor;

                if (IsActionFinished && rayman.State != rayman.Fsm_SuperHelico && BossHealth == 0)
                {
                    State.MoveTo(Fsm_Dying);
                    return false;
                }

                if (IsActionFinished && rayman.State != rayman.Fsm_SuperHelico && BossHealth != 0)
                {
                    ActionId = IsFacingRight ? Action.PrepareCharge_Right : Action.PrepareCharge_Left;
                    State.MoveTo(Fsm_ChargeAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AttackCount = BossHealth == 2 ? 1 : 3;
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__ScalDead_Mix02);
                break;

            case FsmAction.Step:
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 155);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}