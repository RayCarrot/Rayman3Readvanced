using System;

namespace GbaMonoGame.AnimEngine;

public abstract class AObject
{
    // 1100 0000 0000 0000 (0-3)
    public int BgPriority { get; set; }

    // 0011 1111 0000 0000 (0-63)
    public int ObjPriority { get; set; }

    // 0000 0000 1111 1111 (0-255)
    public float YPriority { get; set; }

    // This isn't in the base class in the original game, but easier to manage things this way
    public Vector2 ScreenPos { get; set; }
    public Box RenderBox => new(Vector2.Zero, RenderContext.Resolution);

    // Custom properties to have position scale with camera resolution
    public HorizontalAnchorMode HorizontalAnchor { get; set; }
    public VerticalAnchorMode VerticalAnchor { get; set; }

    public RenderOptions RenderOptions { get; set; }
    public RenderContext RenderContext
    {
        get => RenderOptions.RenderContext;
        set => RenderOptions = RenderOptions with { RenderContext = value };
    }
    public BlendMode BlendMode
    {
        get => RenderOptions.BlendMode;
        set => RenderOptions = RenderOptions with { BlendMode = value };
    }

    public Vector2 GetAnchoredPosition()
    {
        Vector2 pos = ScreenPos;

        switch (HorizontalAnchor)
        {
            default:
            case HorizontalAnchorMode.Left:
                // Do nothing
                break;

            case HorizontalAnchorMode.Center:
                pos.X += RenderContext.Resolution.X / 2;
                break;
            
            case HorizontalAnchorMode.Right:
                pos.X += RenderContext.Resolution.X;
                break;
            
            case HorizontalAnchorMode.Scale:
                pos.X += (RenderContext.Resolution.X - Rom.OriginalResolution.X) / 2;
                break;
        }

        switch (VerticalAnchor)
        {
            default:
            case VerticalAnchorMode.Top:
                // Do nothing
                break;

            case VerticalAnchorMode.Center:
                pos.Y += RenderContext.Resolution.Y / 2;
                break;

            case VerticalAnchorMode.Bottom:
                pos.Y += RenderContext.Resolution.Y;
                break;

            case VerticalAnchorMode.Scale:
                pos.Y += (RenderContext.Resolution.Y - Rom.OriginalResolution.Y) / 2;
                break;
        }

        return pos;
    }

    public abstract void Execute(Action<short> soundEventCallback);
}