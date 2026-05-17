using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class WaterSplash
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                RewindAction();
                ActionId = 1;
                break;

            case FsmAction.Step:
                if (ActionId == 1)
                {
                    if (!Engine.Sem.IsSongPlaying(Rayman3SoundEvent.Play__SplshGen_Mix04))
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__SplshGen_Mix04, this);

                    ActionId = 0;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}