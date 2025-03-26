using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

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

                if (Math.Abs(Position.X - Scene.MainActor.Position.X) < 200)
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
                    Scene.MainActor.ProcessMessage(this, Message.Damaged);
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
                    Scene.MainActor.ProcessMessage(this, Message.Damaged);
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
                // TODO: Implement

                break;

            case FsmAction.Step:
                // TODO: Implement

                break;

            case FsmAction.UnInit:
                // TODO: Implement

                break;
        }

        return true;
    }
}