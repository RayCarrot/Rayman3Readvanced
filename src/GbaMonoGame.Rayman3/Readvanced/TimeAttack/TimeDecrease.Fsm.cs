using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class TimeDecrease
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Reset
                RewindAction();
                AnimatedObject.BlendMode = BlendMode.AlphaBlend;
                AnimatedObject.Alpha = AlphaCoefficient.Max;
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                // Fade out
                if (Timer > 25)
                    AnimatedObject.Alpha = 1 - (Timer - 25) / 15f;

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AnimatedObject.BlendMode = BlendMode.None;
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}