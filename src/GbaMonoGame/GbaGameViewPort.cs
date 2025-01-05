using System;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class GbaGameViewPort
{
    public GbaGameViewPort(GbaEngineSettings settings)
    {
        OriginalGameResolution = settings.Platform switch
        {
            Platform.GBA => new Vector2(240, 160),
            Platform.NGage => new Vector2(176, 208),
            _ => throw new UnsupportedPlatformException(),
        };
        RequestedGameResolution = OriginalGameResolution;
        GameResolution = OriginalGameResolution;
    }

    public Vector2 OriginalGameResolution { get; }
    public Vector2? MinGameResolution { get; private set; }
    public Vector2? MaxGameResolution { get; private set; }
    public Vector2 RequestedGameResolution { get; private set; }
    public Vector2 GameResolution { get; private set; }

    public Box ScreenBox { get; private set; }
    public Rectangle ScreenRectangle { get; private set; }
    public Vector2 ScreenSize { get; private set; }

    private void UpdateGameResolution()
    {
        Vector2 originalGameResolution = GameResolution;
        Vector2 newGameResolution = RequestedGameResolution;

        if (MaxGameResolution is { } max)
        {
            if (newGameResolution.X > max.X)
                newGameResolution = new Vector2(max.X, newGameResolution.Y);
            if (newGameResolution.Y > max.Y)
                newGameResolution = new Vector2(newGameResolution.X, max.Y);
        }

        // If the new resolution is wider than the requested resolution then we crop it down to the same aspect ratio. This
        // is to avoid big black bars on the top and bottom in wider maps such as the worldmap.
        float requestedGameRatio = RequestedGameResolution.X / RequestedGameResolution.Y;
        float newGameRatio = newGameResolution.X / newGameResolution.Y;
        if (newGameRatio > requestedGameRatio)
            newGameResolution = new Vector2(newGameResolution.Y * requestedGameRatio, newGameResolution.Y);

        if (MinGameResolution is { } min)
        {
            if (newGameResolution.X < min.X)
                newGameResolution = new Vector2(min.X, newGameResolution.Y);
            if (newGameResolution.Y < min.Y)
                newGameResolution = new Vector2(newGameResolution.X, min.Y);
        }

        GameResolution = newGameResolution;

        if (GameResolution != originalGameResolution)
        {
            OnGameResolutionChanged();
            Resize(ScreenSize);
        }
    }

    protected virtual void OnResized()
    {
        Resized?.Invoke(this, EventArgs.Empty);
    }

    private void OnGameResolutionChanged()
    {
        GameResolutionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Resize(
        Vector2 newScreenSize,
        bool centerGame = true)
    {
        float screenRatio = newScreenSize.X / newScreenSize.Y;
        float gameRatio = GameResolution.X / GameResolution.Y;

        float screenScale;
        Vector2 screenPos = Vector2.Zero;
        Vector2 screenSize;

        if (screenRatio > gameRatio)
        {
            screenScale = newScreenSize.Y / GameResolution.Y;

            if (centerGame)
                screenPos = new Vector2((newScreenSize.X - GameResolution.X * screenScale) / 2, 0);

            screenSize = new Vector2(GameResolution.X * screenScale, newScreenSize.Y);
        }
        else
        {
            screenScale = newScreenSize.X / GameResolution.X;

            if (centerGame)
                screenPos = new Vector2(0, (newScreenSize.Y - GameResolution.Y * screenScale) / 2);

            screenSize = new Vector2(newScreenSize.X, GameResolution.Y * screenScale);
        }

        ScreenSize = newScreenSize;
        ScreenBox = new Box(screenPos, screenSize);
        ScreenRectangle = new Rectangle(
            (int)MathF.Ceiling(ScreenBox.MinX), 
            (int)MathF.Ceiling(ScreenBox.MinY), 
            (int)MathF.Floor(ScreenBox.MaxX) - (int)MathF.Ceiling(ScreenBox.MinX), 
            (int)MathF.Floor(ScreenBox.MaxY) - (int)MathF.Ceiling(ScreenBox.MinY));

        OnResized();
    }

    public void SetRequestedResolution(Vector2? resolution)
    {
        RequestedGameResolution = resolution ?? OriginalGameResolution;
        UpdateGameResolution();
    }

    public void SetResolutionBoundsToOriginalResolution()
    {
        SetResolutionBounds(OriginalGameResolution, OriginalGameResolution);
    }

    public void SetResolutionBounds(Vector2? minResolution, Vector2? maxResolution)
    {
        MinGameResolution = minResolution;
        MaxGameResolution = maxResolution;
        UpdateGameResolution();
    }

    public event EventHandler Resized;
    public event EventHandler GameResolutionChanged;
}