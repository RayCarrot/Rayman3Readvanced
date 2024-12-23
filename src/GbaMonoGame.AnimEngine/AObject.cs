using System;

namespace GbaMonoGame.AnimEngine;

public abstract class AObject
{
    public ushort Priority { get; private set; }

    // TODO: Do we keep these prio names or rename to ScreenPriority and SpritePriority to match our GFX names?

    // 1100 0000 0000 0000
    public int BgPriority
    {
        get => Priority >> 14;
        set
        {
            if (value > 3)
                throw new Exception("Invalid BG priority! Has to be a value between 0-3.");

            Priority &= 0x3FFF;
            Priority |= (ushort)((value & 0x3) << 14);
        }
    }

    // 0011 1111 0000 0000
    public int ObjPriority
    {
        get => (Priority >> 8) & 0x3F;
        set
        {
            if (value > 63)
                throw new Exception("Invalid OBJ priority! Has to be a value between 0-63.");

            Priority &= 0xC0FF;
            Priority |= (ushort)((value & 0x3F) << 8);
        }
    }

    // 0000 0000 1111 1111
    public int YPriority
    {
        get => Priority & 0xFF;
        set
        {
            if (value > 255)
                throw new Exception("Invalid Y priority! Has to be a value between 0-255.");

            Priority &= 0xFF00;
            Priority |= (ushort)(value & 0xFF);
        }
    }

    // This isn't in the base class in the original game, but easier to manage things this way
    public Vector2 ScreenPos { get; set; }

    // Custom properties to have position scale with camera resolution
    public HorizontalAnchorMode HorizontalAnchor { get; set; }
    public VerticalAnchorMode VerticalAnchor { get; set; }

    public RenderContext RenderContext { get; set; } = Engine.GameRenderContext;

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
                pos.X += (RenderContext.Resolution.X - Engine.GameViewPort.OriginalGameResolution.X) / 2;
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
                pos.Y += (RenderContext.Resolution.Y - Engine.GameViewPort.OriginalGameResolution.Y) / 2;
                break;
        }

        return pos;
    }

    public abstract void Execute(Action<short> soundEventCallback);
}