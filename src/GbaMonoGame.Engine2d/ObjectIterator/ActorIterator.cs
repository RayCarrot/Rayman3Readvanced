using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Engine2d;

public class ActorIterator : ObjectIterator<BaseActor>
{
    public ActorIterator(Scene2D scene) : this(scene, scene.KnotManager.CurrentKnot) { }
    public ActorIterator(Scene2D scene, Knot knot) : base(scene, knot.ActorIds) { }
}