namespace GbaMonoGame.Engine2d;

public class DisabledAlwaysActorIterator : AlwaysActorIterator
{
    public DisabledAlwaysActorIterator(Scene2D scene) : base(scene) { }

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