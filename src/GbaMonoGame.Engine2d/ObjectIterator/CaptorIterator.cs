using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Engine2d;

public class CaptorIterator : ObjectIterator<Captor>
{
    public CaptorIterator(Scene2D scene) : this(scene, scene.KnotManager.CurrentKnot) { }
    public CaptorIterator(Scene2D scene, Knot knot) : base(scene, knot.CaptorIds) { }
}