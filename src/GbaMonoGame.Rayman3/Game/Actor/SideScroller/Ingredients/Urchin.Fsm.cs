using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public partial class Urchin
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // NOTE: All 3 actions are identical, so this does nothing
                ActionId = ActionId switch
                {
                    Action.Idle1 => Action.Idle2,
                    Action.Idle2 => Action.Idle3,
                    Action.Idle3 => Action.Idle1,
                    _ => throw new Exception("Invalid action id")
                };
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                }
                else if (AnimatedObject.IsFramed &&
                         (GameInfo.ActorSoundFlags & ActorSoundFlags.Urchin) == 0 &&
                         IsActionFinished)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BlobFX02_Mix02);
                }

                if (AnimatedObject.IsFramed)
                    GameInfo.ActorSoundFlags |= ActorSoundFlags.Urchin;

                if (IsActionFinished)
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
}