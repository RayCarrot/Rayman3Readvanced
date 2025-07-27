using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class MurfyStone
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                HasTriggered = false;
                break;

            case FsmAction.Step:
                if (Scene.MainActor.GetDetectionBox().Intersects(GetViewBox()) && MurfyId != null)
                {
                    if (!HasTriggered)
                    {
                        if (Timer > 180)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Whistle1_Mix01);
                        
                        Timer = 0;

                        if (Scene.MainActor is Rayman rayman && rayman.State == rayman.Fsm_Default)
                            RaymanIdleTimer++;
                        else
                            RaymanIdleTimer = 0;

                        if (RaymanIdleTimer > 30)
                        {
                            HasTriggered = true;
                            GameObject murfy = Scene.GetGameObject(MurfyId.Value);

                            // The horizontal resolution is incorrectly used here
                            if (Engine.ActiveConfig.Tweaks.FixBugs)
                                murfy.Position = murfy.Position with { Y = Position.Y - Scene.Resolution.Y };
                            else
                                murfy.Position = murfy.Position with { Y = Position.Y - Scene.Resolution.X };
                            murfy.ProcessMessage(this, Message.Murfy_Spawn);
                        }
                    }
                }
                else
                {
                    if (HasTriggered && (Scene.MainActor is not Rayman rayman || rayman.State != rayman.Fsm_Default))
                    {
                        HasTriggered = false;
                        RaymanIdleTimer = 0;
                    }

                    Timer++;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}