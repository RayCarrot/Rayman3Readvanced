using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Balloon
{
    public bool Fsm_Inflate(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Inflate;
                ChangeAction();
                HasPlayedSound = false;
                break;

            case FsmAction.Step:
                if (!HasPlayedSound && AnimatedObject.IsFramed)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BallInit_Mix01);
                    HasPlayedSound = true;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                IsSolid = true;
                ActionId = Action.Idle;
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                Box mainActorDetectionBox = Scene.MainActor.GetDetectionBox();

                // If Rayman is inside the balloon when it inflates
                if (!Scene.IsDetectedMainActor(this))
                {
                    Box detectionBox = GetDetectionBox();

                    if (mainActorDetectionBox.Intersects(detectionBox))
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt, this);
                }

                // Enable next balloon
                if (LinkedBalloonId != null)
                {
                    Box detectionBox = GetDetectionBox();
                    detectionBox.MinY -= 16;
                    detectionBox.MaxY -= 32;

                    if (mainActorDetectionBox.Intersects(detectionBox))
                        Scene.GetGameObject(LinkedBalloonId.Value).ProcessMessage(this, Message.ResurrectWakeUp);
                }

                if (IsTimed && Timer >= 180)
                {
                    State.MoveTo(Fsm_Pop);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Bounce);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Bounce(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                IsSolid = false;

                Box detectionBox = GetDetectionBox();
                Scene.MainActor.Position = Scene.MainActor.Position with { Y = detectionBox.MinY };
                Scene.MainActor.ProcessMessage(this, Message.Rayman_Bounce);
                
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Bounce02_Mix03);
                
                ActionId = Action.Pop;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (Timer <= 180)
                    Timer++;

                if (IsActionFinished)
                    ShouldDraw = false;

                if (!IsRespawnable && IsActionFinished)
                {
                    ProcessMessage(this, Message.Destroy);
                    State.MoveTo(Fsm_Inflate);
                    return false;
                }

                if (Timer > 180)
                {
                    State.MoveTo(Fsm_Inflate);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ShouldDraw = true;
                break;
        }

        return true;
    }

    public bool Fsm_Pop(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Pop;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Inflate);
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