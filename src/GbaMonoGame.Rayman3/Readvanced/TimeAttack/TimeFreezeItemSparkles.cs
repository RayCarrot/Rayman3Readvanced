using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public sealed class TimeFreezeItemSparkles : BaseActor
{
    public TimeFreezeItemSparkles(int instanceId, Scene2D scene, ActorResource actorResource)
        : base(instanceId, scene, actorResource, new AObjectChain(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic))
    {
        AnimatedObject.Init(6, Position, 0, true);
        AnimatedObject.ObjPriority = 32;
        AnimatedObject.BlendMode = BlendMode.AlphaBlend;

        State.SetTo(null);
    }

    public new AObjectChain AnimatedObject => (AObjectChain)base.AnimatedObject;

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        AnimatedObject.Draw(this, animationPlayer, forceDraw);
    }
}