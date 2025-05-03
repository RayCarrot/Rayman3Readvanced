using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class CaptureTheFlagFlagBase
{
    public bool Fsm_Solo(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;
            
            case FsmAction.Step:
                if (!HasReceivedFlag)
                {
                    MessageRefParam<CaptureTheFlagFlag> param = new();
                    AttachedObject.ProcessMessage(this, Message.Rayman_GetPickedUpFlag, param);

                    // If the player has a flag and is colliding with the base...
                    if (param.Value != null && GetViewBox().Intersects(AttachedObject.GetDetectionBox()))
                    {
                        AttachedObject.ProcessMessage(this, Message.Rayman_CaptureFlag);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play_NGage_Unnamed2);
                        ((NGageSoundEventsManager)SoundEventsManager.Current).PauseLoopingSoundEffects();
                        HasReceivedFlag = true;
                        ActionId = Action.Shine;
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SoloCommon(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;
            
            case FsmAction.Step:
                if (!IsFlagTaken)
                {
                    for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                    {
                        Rayman player = Scene.GetGameObject<Rayman>(i);

                        if (GetViewBox().Intersects(player.GetDetectionBox()))
                        {
                            // Give the player the flag
                            player.ProcessMessage(this, Message.Rayman_PickUpFlag, AttachedObject);

                            // Hide the flag from the base animation
                            AnimatedObject.DeactivateChannel(0);

                            // Attach the player to the flag
                            AttachedObject.ProcessMessage(this, Message.CaptureTheFlagFlag_AttachToPlayer, player);
                            
                            IsFlagTaken = true;
                            break;
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

    public bool Fsm_Teams(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;
            
            case FsmAction.Step:
                if (!HasReceivedFlag)
                {
                    for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                    {
                        Rayman player = Scene.GetGameObject<Rayman>(i);

                        // If this is the team's base (where you need to return the flag to after capturing it)
                        if (player == TeamPlayer1 || player == TeamPlayer2)
                        {
                            MessageRefParam<CaptureTheFlagFlag> param = new();
                            player.ProcessMessage(this, Message.Rayman_GetPickedUpFlag, param);

                            // If the player has a flag, it's not the current flag and the player is colliding with the base...
                            if (param.Value != null && param.Value != Flag && GetViewBox().Intersects(player.GetDetectionBox()))
                            {
                                // The flag has been collected
                                ((FrameMultiCaptureTheFlag)Frame.Current).AddFlag(player.InstanceId);
                                HasReceivedFlag = true;
                                ActionId = Action.Shine;
                            }
                        }
                        // If this is the opposite team's base (where you need to capture it from)
                        else
                        {
                            // Cooldown timer after a reset
                            if (Timer != 0)
                                Timer--;

                            // If the flag has not been captured from the base, there's no cooldown timer and the player is colliding with the base...
                            if (!IsFlagTaken && Timer == 0 && GetViewBox().Intersects(player.GetDetectionBox()))
                            {
                                // Give the player the flag
                                player.ProcessMessage(this, Message.Rayman_PickUpFlag, AttachedObject);

                                // Hide the flag from the base animation
                                AnimatedObject.DeactivateChannel(0);

                                // Attach the player to the flag
                                Flag.ProcessMessage(this, Message.CaptureTheFlagFlag_AttachToPlayer, player);

                                IsFlagTaken = true;
                                break;
                            }
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
}