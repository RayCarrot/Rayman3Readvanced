using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class ChainedSparkles : BaseActor
{
    public ChainedSparkles(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource, new AObjectChain(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic))
    {
        AnimatedObject.Init(6, Position, 0, true);
        AnimatedObject.ObjPriority = 10;

        ShouldUpdateTarget = false;

        OriginalTargetActor = Scene.MainActor;
        TargetActor = Scene.MainActor;

        if (!RSMultiplayer.IsActive)
        {
            AreSparklesFacingLeft = false;
            TimerTarget = 360;
            State.SetTo(Fsm_InitSwirl);
        }
        else
        {
            AreSparklesFacingLeft = true;
            TimerTarget = null;
            State.SetTo(Fsm_Wait);
        }
    }

    public new AObjectChain AnimatedObject => (AObjectChain)base.AnimatedObject;

    public static bool ShouldUpdateTarget { get; set; }

    public BaseActor OriginalTargetActor { get; set; }
    public BaseActor TargetActor { get; set; }
    public bool AreSparklesFacingLeft { get; set; }
    public byte SwirlValue { get; set; }
    public ushort Timer { get; set; }
    public ushort? TimerTarget { get; set; }

    public static void UpdateTarget()
    {
        ShouldUpdateTarget = true;
    }

    public void InitNewPower()
    {
        State.SetTo(Fsm_NewPower);
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // Copy over Rayman's alpha blending. The original game doesn't do this since the alpha
        // is global, but here we have to since it's managed per object instead.
        AnimatedObject.Alpha = TargetActor.AnimatedObject.Alpha;

        AnimatedObject.Draw(this, animationPlayer, forceDraw);
    }
}