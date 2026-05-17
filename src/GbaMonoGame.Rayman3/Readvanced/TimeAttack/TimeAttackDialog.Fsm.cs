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
                Rayman3.TimeAttack.SetMode(TimeAttackMode.Countdown);
                break;

            case FsmAction.Step:
                CountdownTimer++;

                switch (CountdownTimer)
                {
                    case CountdownStartTime + CountdownSpeed * 0:
                        SetCountdownValue(3);
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P1_);
                        break;

                    case CountdownStartTime + CountdownSpeed * 1:
                        SetCountdownValue(2);
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P2_);
                        break;

                    case CountdownStartTime + CountdownSpeed * 2:
                        SetCountdownValue(1);
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P3_);
                        break;

                    case CountdownStartTime + CountdownSpeed * 3:
                        SetCountdownValue(0);
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__OnoGO_Mix02);
                        break;

                    case CountdownStartTime + CountdownSpeed * 4:
                        Rayman3.TimeAttack.SetMode(TimeAttackMode.Play);
                        break;
                }

                if (Rayman3.TimeAttack.Mode != TimeAttackMode.Countdown)
                {
                    State.MoveTo(_Fsm_Play);
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
                Rayman3.TimeAttack.InitGhostRecorder(Scene);
                Rayman3.TimeAttack.InitGhostPlayer(Scene);
                break;

            case FsmAction.Step:
                // Increment timer
                Rayman3.TimeAttack.AddTime(1);

                // Update target time
                UpdateTargetTime();

                // Step the ghosts
                Rayman3.TimeAttack.StepGhostRecorder();
                Rayman3.TimeAttack.StepGhostPlayer();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}