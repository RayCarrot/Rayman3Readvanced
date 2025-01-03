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

        UnknownValue = false;

        MainActor1 = Scene.MainActor;
        MainActor2 = Scene.MainActor;

        if (RSMultiplayer.IsActive)
        {
            AreSparklesFacingLeft = true;
            field13_0x38 = 0xFFFF;
            State.SetTo(FUN_08060f58);
        }
        else
        {
            AreSparklesFacingLeft = false;
            field13_0x38 = 360;
            State.SetTo(FUN_08060930);
        }
    }

    public new AObjectChain AnimatedObject => (AObjectChain)base.AnimatedObject;

    public static bool UnknownValue { get; set; } // TODO: Name

    public BaseActor MainActor1 { get; set; }
    public BaseActor MainActor2 { get; set; }
    public bool AreSparklesFacingLeft { get; set; }
    public byte SwirlValue { get; set; }
    public ushort Timer { get; set; }
    public ushort field13_0x38 { get; set; } // TODO: Name

    // TODO: Name
    public static void SetUnknownValue()
    {
        UnknownValue = true;
    }

    public void InitNewPower()
    {
        State.SetTo(Fsm_NewPower);
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        AnimatedObject.Draw(this, animationPlayer, forceDraw);
    }
}