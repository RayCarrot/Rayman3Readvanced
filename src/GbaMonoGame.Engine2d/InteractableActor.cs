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

        _debugAttackBoxAObject = new DebugBoxAObject()
        {
            Color = DebugBoxColor.AttackBox,
            RenderContext = Scene.RenderContext
        };
        _debugVulnerabilityBoxAObject = new DebugBoxAObject()
        {
            Color = DebugBoxColor.VulnerabilityBox,
            RenderContext = Scene.RenderContext
        };
    }

    private readonly DebugBoxAObject _debugAttackBoxAObject;
    private readonly DebugBoxAObject _debugVulnerabilityBoxAObject;
    
    public BoxTable AnimationBoxTable { get; }

    public virtual Box GetAttackBox()
    {
        Box box = AnimationBoxTable.AttackBox;

        if (AnimatedObject.FlipX)
            box = Box.FlipX(box);

        if (AnimatedObject.FlipY)
            box = Box.FlipY(box);

        return Box.Offset(box, Position);
    }

    public virtual Box GetVulnerabilityBox()
    {
        Box box = AnimationBoxTable.VulnerabilityBox;

        if (AnimatedObject.FlipX)
            box = Box.FlipX(box);

        if (AnimatedObject.FlipY)
            box = Box.FlipY(box);

        return Box.Offset(box, Position);
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        Box attackBox = GetAttackBox();
        if (Scene.Camera.IsDebugBoxFramed(_debugAttackBoxAObject, attackBox.Position))
        {
            _debugAttackBoxAObject.Size = attackBox.Size;
            animationPlayer.PlayFront(_debugAttackBoxAObject);
        }

        Box vulnerabilityBox = GetVulnerabilityBox();
        if (Scene.Camera.IsDebugBoxFramed(_debugVulnerabilityBoxAObject, vulnerabilityBox.Position))
        {
            _debugVulnerabilityBoxAObject.Size = vulnerabilityBox.Size;
            animationPlayer.PlayFront(_debugVulnerabilityBoxAObject);
        }
    }
}