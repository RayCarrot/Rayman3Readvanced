using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Editor;

public class EditableActor : EditableGameObject
{
    public EditableActor(EditableScene2D scene, ActorResource actorResource) : base(scene, actorResource)
    {
        ActorResource = actorResource;
        ActorModel = actorResource.Model;

        AnimatedObject = new AnimatedObject(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic)
        {
            CurrentAnimation = actorResource.Model.Actions.ElementAtOrDefault(actorResource.FirstActionId)?.AnimationIndex ?? actorResource.FirstActionId,
            BgPriority = 1,
            ObjPriority = 32,
            RenderContext = scene.Camera.RenderContext
        };

        _viewBox = new Box(ActorModel.ViewBox);
    }

    private readonly Box _viewBox;

    public ActorResource ActorResource { get; }
    public ActorModel ActorModel { get; }
    public AnimatedObject AnimatedObject { get; }

    public Box GetViewBox() => _viewBox.Offset(Position);
    public override Box GetSelectionBox() => GetViewBox();

    public void Draw(AnimationPlayer animationPlayer)
    {
        if (Scene.Camera.IsActorFramed(this))
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