namespace GbaMonoGame.Engine2d;

public class AlwaysActorIterator : ObjectIterator<BaseActor>
{
    public AlwaysActorIterator(Scene2D scene) : base(scene, null)
    {
        Index = KnotManager.AlwaysActorsIndex;
    }

    public override bool MoveNext()
    {
        if (!base.MoveNext())
            return false;

        // If we've just finished enumerating the always actors...
        if (Index == KnotManager.AlwaysActorsIndex + KnotManager.AlwaysActorsCount)
        {
            // ...then we start enumerating the added projectiles instead (they're at the very end)
            Index = KnotManager.AddedProjectilesIndex;
        }

        return true;
    }
}