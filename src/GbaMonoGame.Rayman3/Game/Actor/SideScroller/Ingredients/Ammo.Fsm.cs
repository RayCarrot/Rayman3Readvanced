using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Ammo
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (Timer != 120)
                {
                    Timer++;
                }
                else
                {
                    Box viewBox = GetViewBox();
                    Box mainActorVulnerabilityBox = Scene.Camera.LinkedObject.GetVulnerabilityBox();

                    if (viewBox.Intersects(mainActorVulnerabilityBox))
                    {
                        Scene.Camera.LinkedObject.ProcessMessage(this, Message.FlyingShell_RefillAmmo);
                        Scene.GetGameObject(1).ProcessMessage(this, Message.FlyingShell_RefillAmmo);
                        Timer = 0;

                        float[] yPositions = [90, 170, 260];
                        Position = Position with { Y = yPositions[Random.GetNumber(3)] };
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