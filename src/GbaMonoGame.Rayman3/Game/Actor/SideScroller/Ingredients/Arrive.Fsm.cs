using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Arrive
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                    State.MoveTo(Fsm_EndLevel);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_IdleWithLink(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                bool skipCutscene = false;
                bool endLevel = false;

                if (Scene.IsDetectedMainActor(this))
                {
                    if (GameInfo.MapId == MapId.ChallengeLy1 && !GameInfo.PersistentInfo.FinishedLyChallenge1)
                    {
                        if (Rom.Platform == Platform.GBA)
                            Scene.GetGameObject(LinkedActor!.Value).ProcessMessage(this, Message.Murfy_Spawn);

                        GameInfo.PersistentInfo.FinishedLyChallenge1 = true;
                    }
                    else if (GameInfo.MapId == MapId.ChallengeLy2 && !GameInfo.PersistentInfo.FinishedLyChallenge2)
                    {
                        if (Rom.Platform == Platform.GBA)
                            Scene.GetGameObject(LinkedActor!.Value).ProcessMessage(this, Message.Murfy_Spawn);

                        GameInfo.PersistentInfo.FinishedLyChallenge2 = true;
                    }
                    else if (GameInfo.MapId == MapId.ChallengeLyGCN)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginCutscene);
                        GameInfo.PersistentInfo.FinishedLyChallengeGCN = true;
                        skipCutscene = true;
                    }

                    endLevel = true;

                    ((FrameSideScroller)Frame.Current).IsTimed = false;
                }

                if (skipCutscene)
                {
                    State.MoveTo(Fsm_EndLevel);
                    return false;
                }

                if (endLevel)
                {
                    if (Rom.Platform == Platform.GBA)
                        State.MoveTo(Fsm_Cutscene);
                    else
                        State.MoveTo(Fsm_EndLevel);

                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Cutscene(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                if (Scene.GetDialog<TextBoxDialog>().IsFinished)
                {
                    State.MoveTo(Fsm_EndLevel);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_EndLevel(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.EndingLevel;
                break;

            case FsmAction.Step:
                if (IsActionFinished && ActionId == Action.EndingLevel)
                {
                    if (Rom.Platform == Platform.GBA)
                    {
                        if (GameInfo.MapId == MapId.ChallengeLyGCN)
                            Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                    }
                    else if (Rom.Platform == Platform.NGage)
                    {
                        if (GameInfo.MapId is MapId.ChallengeLy1 or MapId.ChallengeLy2 or MapId.ChallengeLyGCN)
                            Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    ActionId = Action.EndedLevel;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}