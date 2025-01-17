namespace GbaMonoGame.Editor;

public class EditorRenderContext : RenderContext
{
    public float Scale { get; set; }
    public Vector2 MaxResolution { get; set; }

    protected override Vector2 GetResolution()
    {
        Vector2 newGameResolution = Engine.Config.InternalGameResolution * Scale;

        Vector2 max = MaxResolution;

        if (newGameResolution.X > newGameResolution.Y)
        {
            if (newGameResolution.Y > max.Y)
                newGameResolution = new Vector2(max.Y * newGameResolution.X / newGameResolution.Y, max.Y);

            if (newGameResolution.X > max.X)
                newGameResolution = new Vector2(max.X, max.X * newGameResolution.Y / newGameResolution.X);
        }
        else
        {
            if (newGameResolution.X > max.X)
                newGameResolution = new Vector2(max.X, max.X * newGameResolution.Y / newGameResolution.X);

            if (newGameResolution.Y > max.Y)
                newGameResolution = new Vector2(max.Y * newGameResolution.X / newGameResolution.Y, max.Y);
        }

        return newGameResolution;
    }
}