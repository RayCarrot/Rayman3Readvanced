using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class LumsMode7 : Mode7Actor
{
    public LumsMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 32;
        Timer = MaxTimer;

        if (RSMultiplayer.IsActive)
        {
            State.SetTo(Fsm_MultiplayerIdle);
        }
        else
        {
            State.SetTo(Fsm_Idle);

            if (ActionId == Action.YellowLum)
            {
                LumId = GameInfo.GetLumsId();

                if (GameInfo.IsLumDead(LumId, GameInfo.MapId))
                    ProcessMessage(this, Message.Destroy);
            }
        }
    }

    private const int MaxTimer = 250;

    public int LumId { get; }
    public int Timer { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if ((Scene.Camera.IsActorFramed(this) || forceDraw) && Timer == MaxTimer)
        {
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }
}