using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class LumsMode7
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this) && ((Mode7Actor)Scene.MainActor).ZPos <= 23)
                {
                    State.MoveTo(Fsm_Collected);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Collected(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (ActionId == Action.YellowLum)
                {
                    Scene.MainActor.ProcessMessage(this, Message.MainMode7_CollectedYellowLum);

                    if (GameInfo.LevelType == LevelType.Race)
                        ((FrameSingleMode7)Frame.Current).KillLum(LumId);
                    else
                        GameInfo.SetYellowLumAsCollected(LumId);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumOrag_Mix06);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumOrag_Mix06);

                    ProcessMessage(this, Message.Destroy);
                }
                // Unused
                else if (!RSMultiplayer.IsActive && ActionId == Action.BlueLum)
                {
                    Scene.MainActor.ProcessMessage(this, Message.MissileMode7_CollectedBlueLum);
                }
                else if (!RSMultiplayer.IsActive && ActionId == Action.RedLum)
                {
                    Scene.MainActor.ProcessMessage(this, Message.MissileMode7_CollectedRedLum);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumRed_Mix03);
                }

                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                if (RSMultiplayer.IsActive && Timer == MaxTimer)
                {
                    State.MoveTo(Fsm_MultiplayerIdle);
                    return false;
                }

                if (!RSMultiplayer.IsActive && Timer == MaxTimer)
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

    public bool Fsm_MultiplayerIdle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                bool collected = false;
                
                Box actionBox = GetActionBox();

                for (int id = (int)(MultiplayerManager.GetElapsedTime() % 2); id < MultiplayerManager.PlayersCount; id += 2)
                {
                    Mode7Actor player = Scene.GetGameObject<Mode7Actor>(id);
                    Box playerDetectionBox = player.GetDetectionBox();

                    if (player.HitPoints != 0 && actionBox.Intersects(playerDetectionBox) && player.ZPos < 23)
                    {
                        if (ActionId == Action.BlueLum)
                        {
                            player.ProcessMessage(this, Message.MissileMode7_CollectedBlueLum);
                        }
                        else if (ActionId == Action.RedLum)
                        {
                            player.ProcessMessage(this, Message.MissileMode7_CollectedRedLum);
                         
                            if (id == MultiplayerManager.MachineId)
                                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumRed_Mix03);
                        }

                        collected = true;
                    }
                }

                if (collected)
                {
                    State.MoveTo(Fsm_Collected);
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