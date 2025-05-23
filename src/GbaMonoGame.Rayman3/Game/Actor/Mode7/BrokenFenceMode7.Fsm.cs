﻿using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class BrokenFenceMode7
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this) && ((Mode7Actor)Scene.MainActor).ZPos < 16)
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}