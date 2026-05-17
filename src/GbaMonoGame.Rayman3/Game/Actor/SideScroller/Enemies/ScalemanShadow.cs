using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed class ScalemanShadow : BaseActor
{
    public ScalemanShadow(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource)
    {
        // Optionally make the shadow render behind the boss to avoid it overlapping on top of it
        if (Engine.Config.Active.Tweaks.VisualImprovements)
            AnimatedObject.ObjPriority = 33;
        
        State.SetTo(null);
    }
}