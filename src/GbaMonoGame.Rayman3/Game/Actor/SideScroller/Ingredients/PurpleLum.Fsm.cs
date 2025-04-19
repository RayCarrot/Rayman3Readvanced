using System;
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

                if (RSMultiplayer.IsActive && Rom.Platform == Platform.NGage)
                {
                    throw new NotImplementedException();
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

                        viewBox.MinX += 16;
                        viewBox.MinY += 8;
                        viewBox.MaxX -= 16;
                        viewBox.MaxY += 4;

                        if (!detectionBox.Intersects(viewBox)) 
                            continue;
                        
                        rayman.ProcessMessage(this, Message.Main_BeginSwing, this);
                        activeFist.ProcessMessage(this, Message.RaymanBody_FinishedAttack, this);
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