using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Engine2d;

public class ActorCaptorIterator : ObjectIterator<GameObject>
{
    public ActorCaptorIterator(Scene2D scene) : this(scene, scene.KnotManager.CurrentKnot) { }
    public ActorCaptorIterator(Scene2D scene, Knot knot) : base(scene, knot.ActorIds.Concat(knot.CaptorIds).ToArray()) { }
}