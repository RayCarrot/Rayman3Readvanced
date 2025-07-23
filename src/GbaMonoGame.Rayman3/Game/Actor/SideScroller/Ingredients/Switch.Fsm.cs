using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Switch
{
    public bool Fsm_Deactivated(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (HitPoints == 0)
                {
                    State.MoveTo(Fsm_Activating);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Activating(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Activating;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);

                if (Links[0] == null)
                {
                    LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win1);
                }
                else
                {
                    foreach (byte? link in Links)
                    {
                        if (link == null)
                            break;

                        Scene.GetGameObject(link.Value).ProcessMessage(this, Message.Gate_Open);
                    }
                }

                if (GameInfo.MapId == MapId.EchoingCaves_M1)
                {
                    ((FrameSideScroller)Frame.Current).UserInfo.SwitchActivated();
                }
                else if (GameInfo.MapId == MapId.BeneathTheSanctuary_M1)
                {
                    // Trigger captor when switch is hit
                    if (Rom.Platform == Platform.NGage && Engine.ActiveConfig.Tweaks.FixBugs)
                    {
                        // Fix object IDs to match on N-Gage
                        switch (InstanceId)
                        {
                            case 14:
                                Scene.GetGameObject(129).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(129).ProcessMessage(this, Message.Captor_Trigger);
                                break;

                            case 18:
                                Scene.GetGameObject(130).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(130).ProcessMessage(this, Message.Captor_Trigger);
                                break;

                            case 22:
                                Scene.GetGameObject(131).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(131).ProcessMessage(this, Message.Captor_Trigger);
                                break;

                            case 23:
                                Scene.GetGameObject(133).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(133).ProcessMessage(this, Message.Captor_Trigger);
                                break;
                        }
                    }
                    else
                    {
                        switch (InstanceId)
                        {
                            case 14:
                                Scene.GetGameObject(129).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(129).ProcessMessage(this, Message.Captor_Trigger);
                                break;

                            case 21:
                                Scene.GetGameObject(130).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(130).ProcessMessage(this, Message.Captor_Trigger);
                                break;

                            case 27:
                                Scene.GetGameObject(131).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(131).ProcessMessage(this, Message.Captor_Trigger);
                                break;

                            case 28:
                                Scene.GetGameObject(133).ProcessMessage(this, Message.Resurrect);
                                Scene.GetGameObject(133).ProcessMessage(this, Message.Captor_Trigger);
                                break;
                        }
                    }
                }
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Activated);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Activated(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Activated;
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}