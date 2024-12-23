using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Engine2d;

public abstract class InteractableActor : ActionActor
{
    protected InteractableActor(int instanceId, Scene2D scene, ActorResource actorResource)
        : this(instanceId, scene, actorResource, new AnimatedObject(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic)) { }

    protected InteractableActor(int instanceId, Scene2D scene, ActorResource actorResource, AnimatedObject animatedObject)
        : base(instanceId, scene, actorResource, animatedObject)
    {
        _animationBoxTable = new BoxTable();
        AnimatedObject.BoxTable = _animationBoxTable;

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

    private readonly BoxTable _animationBoxTable;
    private readonly DebugBoxAObject _debugAttackBoxAObject;
    private readonly DebugBoxAObject _debugVulnerabilityBoxAObject;

    public virtual Box GetAttackBox()
    {
        Box box = _animationBoxTable.AttackBox;

        if (AnimatedObject.FlipX)
            box = box.FlipX();

        if (AnimatedObject.FlipY)
            box = box.FlipY();

        return box.Offset(Position);
    }

    public virtual Box GetVulnerabilityBox()
    {
        Box box = _animationBoxTable.VulnerabilityBox;

        if (AnimatedObject.FlipX)
            box = box.FlipX();

        if (AnimatedObject.FlipY)
            box = box.FlipY();

        return box.Offset(Position);
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        _debugAttackBoxAObject.Position = Position + _animationBoxTable.AttackBox.Position - Scene.Playfield.Camera.Position;
        _debugAttackBoxAObject.Size = _animationBoxTable.AttackBox.Size;
        animationPlayer.PlayFront(_debugAttackBoxAObject);

        _debugVulnerabilityBoxAObject.Position = Position + _animationBoxTable.VulnerabilityBox.Position - Scene.Playfield.Camera.Position;
        _debugVulnerabilityBoxAObject.Size = _animationBoxTable.VulnerabilityBox.Size;
        animationPlayer.PlayFront(_debugVulnerabilityBoxAObject);
    }
}