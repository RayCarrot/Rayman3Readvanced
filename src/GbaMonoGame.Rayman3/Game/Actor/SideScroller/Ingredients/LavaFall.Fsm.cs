using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class LavaFall
{
    public bool Fsm_Flow(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ShouldDraw = true;
                ActionId = Action.BeginFlow;
                break;

            case FsmAction.Step:
                Timer++;

                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Exploded);

                if (Timer == 60)
                {
                    if (AnimatedObject.IsFramed)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LavaStrt_Mix04);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LavaStrt_Mix04);
                    }

                    BubbleSoundCountdown = 60;
                }
                else if (Timer > 60)
                {
                    if (BubbleSoundCountdown == 0)
                    {
                        if (AnimatedObject.IsFramed && (GameInfo.ActorSoundFlags & ActorSoundFlags.LavaFall) == 0)
                        {
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LavaBubl_Mix02);
                            BubbleSoundCountdown = (byte)(Random.GetNumber(24) + 36);
                        }
                    }
                    else
                    {
                        BubbleSoundCountdown--;
                    }

                    if (AnimatedObject.IsFramed)
                        GameInfo.ActorSoundFlags |= ActorSoundFlags.LavaFall;
                }

                if (ActionId == Action.BeginFlow && IsActionFinished)
                    ActionId = Action.Flow;

                if (Timer > 240)
                {
                    State.MoveTo(Fsm_Wait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ActionId = Action.EndFlow;

                if (AnimatedObject.IsFramed)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LavaStrt_Mix04);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LavaStrt_Mix04);
                }
                break;
        }

        return true;
    }

    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                if (IsActionFinished)
                    ShouldDraw = false;

                if (Timer > 240)
                {
                    State.MoveTo(Fsm_Flow);
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