using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class PurpleLum
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                Box viewBox = GetViewBox();

                // Unused - purple lums don't appear in multiplayer
                if (RSMultiplayer.IsActive && Rom.Platform == Platform.NGage)
                {
                    for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                    {
                        Rayman player = Scene.GetGameObject<Rayman>(i);

                        // NOTE: There's a bug here - it reuses the index from the outer loop! Not worth fixing though since it's unused.
                        for (i = 0; i < 2; i++)
                        {
                            RaymanBody activeFist = player.ActiveBodyParts[i];

                            if (activeFist == null) 
                                continue;

                            Box detectionBox = activeFist.GetDetectionBox();
                            if (!detectionBox.Intersects(viewBox)) 
                                continue;
                            
                            viewBox.Left += 16;
                            viewBox.Top += 8;
                            viewBox.Right -= 16;
                            viewBox.Bottom += 4;

                            if (!detectionBox.Intersects(viewBox)) 
                                continue;
                            
                            player.ProcessMessage(this, Message.Rayman_BeginSwing, this);
                            activeFist.ProcessMessage(this, Message.RaymanBody_FinishAttack, this);
                            break;
                        }
                    }
                }
                else
                {
                    Rayman rayman = (Rayman)Scene.MainActor;

                    // Why is this code so weird with how the view box is handled?
                    for (int i = 0; i < 2; i++)
                    {
                        RaymanBody activeFist = rayman.ActiveBodyParts[i];

                        if (activeFist == null)
                            continue;

                        Box detectionBox = activeFist.GetDetectionBox();
                        if (!detectionBox.Intersects(viewBox)) 
                            continue;

                        viewBox.Left += 16;
                        viewBox.Top += 8;
                        viewBox.Right -= 16;
                        viewBox.Bottom += 4;

                        if (!detectionBox.Intersects(viewBox)) 
                            continue;
                        
                        rayman.ProcessMessage(this, Message.Rayman_BeginSwing, this);
                        activeFist.ProcessMessage(this, Message.RaymanBody_FinishAttack, this);
                        break;
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