using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class LumsMode7 : Mode7Actor
{
    public LumsMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = POSITION_Z;
        RenderHeight = RENDER_HEIGHT;
        Timer = RESPAWN_TIME;

        if (RSMultiplayer.IsActive)
        {
            State.SetTo(_Fsm_MultiplayerIdle);
        }
        else
        {
            State.SetTo(_Fsm_Idle);

            if (ActionId == Action.YellowLum)
            {
                LumId = Rayman3.GameInfo.GetLumsId();

                if (Rayman3.GameInfo.IsLumDead(LumId, Rayman3.GameInfo.MapId))
                    ProcessMessage(this, Message.Destroy);
            }
        }
    }

    public int LumId { get; }
    public int Timer { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if ((Scene.Camera.IsActorFramed(this) || forceDraw) && Timer == RESPAWN_TIME)
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