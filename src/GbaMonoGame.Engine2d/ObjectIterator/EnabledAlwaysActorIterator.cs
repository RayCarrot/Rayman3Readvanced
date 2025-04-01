namespace GbaMonoGame.Engine2d;

public class EnabledAlwaysActorIterator : AlwaysActorIterator
{
    public EnabledAlwaysActorIterator(Scene2D scene) : base(scene) { }

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