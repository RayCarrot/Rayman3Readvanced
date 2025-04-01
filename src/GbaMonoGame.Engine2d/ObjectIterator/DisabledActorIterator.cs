using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Engine2d;

public class DisabledActorIterator : ActorIterator
{
    public DisabledActorIterator(Scene2D scene) : base(scene, scene.KnotManager.CurrentKnot) { }
    public DisabledActorIterator(Scene2D scene, Knot knot) : base(scene, knot) { }

    public override bool MoveNext()
    {
        while (base.MoveNext())
        {
            if (!Current!.IsEnabled)
                return true;
        }

        return false;
    }
}