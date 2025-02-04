using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Engine2d;

public abstract class CameraActor : Object
{
    protected CameraActor(Scene2D scene)
    {
        Scene = scene;
    }

    protected FiniteStateMachine State { get; } = new();

    public Scene2D Scene { get; }
    public MovableActor LinkedObject { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        return false;
    }

    public virtual void Step()
    {
        State.Step();
    }

    public abstract void SetFirstPosition();
    public abstract bool IsActorFramed(BaseActor actor);
    public abstract bool IsDebugBoxFramed(AObject obj, Vector2 position);
}