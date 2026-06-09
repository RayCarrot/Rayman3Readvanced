using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Engine2d;

public abstract class InteractableActor : ActionActor
{
    protected InteractableActor(int instanceId, Scene2D scene, ActorResource actorResource)
        : this(instanceId, scene, actorResource, new AnimatedObject(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic)) { }

    protected InteractableActor(int instanceId, Scene2D scene, ActorResource actorResource, AnimatedObject animatedObject)
        : base(instanceId, scene, actorResource, animatedObject)
    {
        AnimationBoxTable = new BoxTable();
        AnimatedObject.BoxTable = AnimationBoxTable;

        _debugBoxTableAObject = new DebugBoxTableAObject(this)
        {
            RenderContext = Scene.RenderContext
        };
    }

    private readonly DebugBoxTableAObject _debugBoxTableAObject;
    
    public BoxTable AnimationBoxTable { get; }

    public virtual Box GetAttackBox()
    {
        Box box = AnimationBoxTable.AttackBox;

        if (AnimatedObject.FlipX)
            box = Box.FlipX(box);

        if (AnimatedObject.FlipY)
            box = Box.FlipY(box);

        return Box.Offset(box, Position.Truncate());
    }

    public virtual Box GetVulnerabilityBox()
    {
        Box box = AnimationBoxTable.VulnerabilityBox;

        if (AnimatedObject.FlipX)
            box = Box.FlipX(box);

        if (AnimatedObject.FlipY)
            box = Box.FlipY(box);

        return Box.Offset(box, Position.Truncate());
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        // NOTE: Not checking if is framed, but not needed since it's just for debugging.
        //       Also force bg priority to 3 and to not use PlayFront so this gets processed
        //       last, after the box table has updated!
        _debugBoxTableAObject.BgPriority = 3;
        animationPlayer.Play(_debugBoxTableAObject);
    }
}