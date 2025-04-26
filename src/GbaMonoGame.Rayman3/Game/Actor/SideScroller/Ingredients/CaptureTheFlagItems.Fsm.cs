using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using SharpDX.Direct2D1.Effects;

namespace GbaMonoGame.Rayman3;

public partial class CaptureTheFlagItems
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (Timer != 0)
                {
                    Timer--;
                }
                else
                {
                    Box viewBox = GetViewBox();

                    for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                    {
                        Rayman player = Scene.GetGameObject<Rayman>(id);
                        Box playerDetectionBox = player.GetDetectionBox();

                        if (viewBox.Intersects(playerDetectionBox))
                        {
                            Timer = 420;

                            // NOTE: The game passes in two values as a param, the action and also the duration. Since the duration
                            //       is always the same though it's easier to just hard-code it when processing the message.
                            player.ProcessMessage(this, Message.Rayman_CollectCaptureTheFlagItem, InitialActionId);
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
}