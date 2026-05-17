using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class LevelCurtain
{
    public bool Fsm_Locked(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    if ((JoyPad.IsButtonJustPressed(Rayman3Input.ActorUp) || JoyPad.IsButtonJustPressed(Rayman3Input.ActorJump)) && 
                        JoyPad.IsButtonReleased(Rayman3Input.ActorLeft) &&
                        JoyPad.IsButtonReleased(Rayman3Input.ActorRight) &&
                        !((World)Frame.Current).UserInfo.Hide)
                    {
                        MovableActor mainActor = Scene.MainActor;

                        if (mainActor.Speed.Y == 0)
                        {
                            if (!Engine.Sem.IsSongPlaying(Rayman3SoundEvent.Play__Tag_Mix02))
                                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Tag_Mix02, this);

                            mainActor.ProcessMessage(this, Message.Rayman_EnterLockedLevel);
                        }
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Unlocked(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                bool enterCurtain = false;

                if (Scene.IsDetectedMainActor(this) && Scene.MainActor.Speed.Y == 0)
                {
                    ((World)Frame.Current).UserInfo.SetLevelInfoBar((int)InitialActionId);

                    // Optionally fix bug where multiple curtains conflict with each other if active at once
                    if (!Engine.Config.Active.Tweaks.FixBugs || !IsRaymanInFront)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginInFrontOfLevelCurtain);
                        IsRaymanInFront = true;
                    }

                    if ((JoyPad.IsButtonPressed(Rayman3Input.ActorUp) || JoyPad.IsButtonPressed(Rayman3Input.ActorJump)) &&
                        JoyPad.IsButtonReleased(Rayman3Input.ActorLeft) &&
                        JoyPad.IsButtonReleased(Rayman3Input.ActorRight) &&
                        !((World)Frame.Current).UserInfo.Hide)
                    {
                        enterCurtain = true;
                    }
                    else
                    {
                        Rayman rayman = (Rayman)Scene.MainActor;
                        if (ActionId != Action.Sparkle && rayman.State != rayman._Fsm_Default)
                            ActionId = Action.Sparkle;
                        else if (IsActionFinished)
                            ActionId = InitialActionId;
                    }
                }
                else
                {
                    // Optionally fix bug where multiple curtains conflict with each other if active at once
                    if (!Engine.Config.Active.Tweaks.FixBugs || IsRaymanInFront)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_EndInFrontOfLevelCurtain);
                        IsRaymanInFront = false;
                    }
                }

                if (enterCurtain)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
                    State.MoveTo(_Fsm_EnterCurtain);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_EnterCurtain(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (ActionId != Action.Sparkle)
                {
                    ActionId = Action.EnterCurtain1;
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_EnterLevel);
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Curtain_YoyoMove_Mix02, this);
                }
                break;

            case FsmAction.Step:
                bool transitionToLevel = false;

                if (IsActionFinished)
                {
                    if (ActionId == Action.Sparkle)
                    {
                        ActionId = Action.EnterCurtain1;
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_EnterLevel);
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Curtain_YoyoMove_Mix02, this);
                    }
                    else if (ActionId == Action.EnterCurtain1)
                    {
                        AnimatedObject.ObjPriority = 0;
                        ActionId = Action.EnterCurtain2;
                    }
                    else
                    {
                        transitionToLevel = true;
                    }
                }

                if (transitionToLevel)
                {
                    State.MoveTo(_Fsm_TransitionToLevel);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_TransitionToLevel(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ((World)Frame.Current).InitExiting();
                ActionId = InitialActionId;
                break;

            case FsmAction.Step:
                if (((World)Frame.Current).FinishedTransitioningOut)
                {
                    State.MoveTo(_Fsm_Unlocked);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                Gfx.Fade = AlphaCoefficient.Max;

                Engine.Sem.StopAllSongs();
                GameInfo.LoadLevel(MapId);
                break;
        }

        return true;
    }
}