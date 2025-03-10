using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class Playfield2DRenderContext : RenderContext
{
    public Playfield2DRenderContext(Vector2? minResolution, Vector2? maxResolution)
    {
        _minResolution = minResolution;
        _maxResolution = maxResolution;
    }

    private Vector2? _minResolution;
    private Vector2? _maxResolution;

    public Vector2? MinResolution
    {
        get => _minResolution;
        set
        {
            _minResolution = value;
            UpdateResolution();
        }
    }
    public Vector2? MaxResolution
    {
        get => _maxResolution;
        set
        {
            _maxResolution = value;
            UpdateResolution();
        }
    }

    protected override Vector2 GetResolution()
    {
        Vector2 resolution = Engine.InternalGameResolution;

        if (MaxResolution is { } max)
        {
            if (resolution.X > max.X)
                resolution = new Vector2(max.X, resolution.Y);
            if (resolution.Y > max.Y)
                resolution = new Vector2(resolution.X, max.Y);
        }

        // If the new resolution is wider than the requested resolution then we crop it down to the same aspect ratio. This
        // is to avoid big black bars on the top and bottom in wider maps such as the worldmap.
        float requestedGameRatio = Engine.InternalGameResolution.X / Engine.InternalGameResolution.Y;
        float newGameRatio = resolution.X / resolution.Y;
        if (newGameRatio > requestedGameRatio)
            resolution = new Vector2(resolution.Y * requestedGameRatio, resolution.Y);

        if (MinResolution is { } min)
        {
            if (resolution.X < min.X)
                resolution = new Vector2(min.X, resolution.Y);
            if (resolution.Y < min.Y)
                resolution = new Vector2(resolution.X, min.Y);
        }

        return resolution;
    }

    public void SetFixedResolution(Vector2 fixedResolution)
    {
        _minResolution = fixedResolution;
        _maxResolution = fixedResolution;
        UpdateResolution();
    }
}