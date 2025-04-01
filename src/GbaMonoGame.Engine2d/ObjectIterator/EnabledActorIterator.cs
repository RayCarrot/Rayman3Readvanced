using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Engine2d;

public class EnabledActorIterator : ActorIterator
{
    public EnabledActorIterator(Scene2D scene) : this(scene, scene.KnotManager.CurrentKnot) { }
    public EnabledActorIterator(Scene2D scene, Knot knot) : base(scene, knot) { }

    public override bool MoveNext()
    {
        while (base.MoveNext())
        {
            if (Current!.IsEnabled)
                return true;
        }

        return false;
    }
}