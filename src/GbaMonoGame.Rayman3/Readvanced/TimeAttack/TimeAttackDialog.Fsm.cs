using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class TimeAttackDialog
{
    public bool Fsm_Countdown(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                CountdownValue = -1;
                TimeAttackInfo.Mode = TimeAttackMode.Countdown;
                break;

            case FsmAction.Step:
                CountdownTimer++;

                switch (CountdownTimer)
                {
                    case CountdownStartTime + CountdownSpeed * 0:
                        SetCountdownValue(3);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P1_);
                        break;

                    case CountdownStartTime + CountdownSpeed * 1:
                        SetCountdownValue(2);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P2_);
                        break;

                    case CountdownStartTime + CountdownSpeed * 2:
                        SetCountdownValue(1);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P3_);
                        break;

                    case CountdownStartTime + CountdownSpeed * 3:
                        SetCountdownValue(0);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoGO_Mix02);
                        break;

                    case CountdownStartTime + CountdownSpeed * 4:
                        TimeAttackInfo.Mode = TimeAttackMode.Play;
                        break;
                }

                if (TimeAttackInfo.Mode != TimeAttackMode.Countdown)
                {
                    State.MoveTo(Fsm_Play);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Play(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                // TODO: We can't allow it to go too high or the game might crash - kill Rayman if it reaches some large value
                TimeAttackInfo.Timer++;

                // Update target time
                if (TargetTimeIndex != -1 && TimeAttackInfo.Timer > TargetTime.Time)
                    SetTargetTime(TargetTimeIndex - 1);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}