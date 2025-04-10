using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Electricity
{
    public bool Fsm_Activated(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = InitialActionId;
                break;

            case FsmAction.Step:
                if (AnimatedObject.IsFramed)
                {
                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__Electric_Mix02))
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Electric_Mix02);

                    GameInfo.ActorSoundFlags |= ActorSoundFlags.Electricity;
                }

                if (Scene.IsHitMainActor(this) ||
                    (InitialActionId is Action.DoubleActivated_Left or Action.DoubleActivated_Right &&
                     Scene.MainActor.GetVulnerabilityBox().Intersects(AdditionalAttackBox)))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    Scene.MainActor.ProcessMessage(this, Message.Main_DamagedShock);
                }

                if (HitPoints == 0)
                {
                    State.MoveTo(Fsm_Deactivated);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Deactivated(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = InitialActionId switch
                {
                    Action.SingleActivated_Left => Action.SingleDeactivated_Left,
                    Action.SingleActivated_Right => Action.SingleDeactivated_Right,
                    Action.DoubleActivated_Left => Action.DoubleDeactivated_Left,
                    Action.DoubleActivated_Right => Action.DoubleDeactivated_Right,
                    _ => throw new Exception("Invalid initial action id")
                };
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    Scene.MainActor.ProcessMessage(this, Message.Main_DamagedShock);
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}