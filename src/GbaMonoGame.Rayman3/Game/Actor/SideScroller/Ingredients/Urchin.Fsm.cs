using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public partial class Urchin
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // TODO: What's the point of this? All 3 actions are identical... There are
                //       however unused animations - maybe they were meant to play in a cycle?
                ActionId = (ActionId + 1) % 3;
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