using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

// TODO: Rename to something like GbaViewPort or VirtualViewPort?
public abstract class RenderContext
{
    private Box _lastViewPortRenderBox;
    private Vector2 _lastViewPortFullSize;
    private Vector2 _lastGameResolution;

    private Vector2 _resolution;

    protected virtual HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Center;
    protected virtual VerticalAlignment VerticalAlignment => VerticalAlignment.Center;
    protected virtual bool FitToGameViewPort => true;

    public Vector2 Resolution
    {
        get
        {
            Update();
            return _resolution;
        }
        private set => _resolution = value;
    }

    public float AspectRatio => Viewport.AspectRatio;

    public float Scale
    {
        get
        {
            Update();
            return field;
        }
        private set;
    }

    public Viewport Viewport
    {
        get
        {
            Update();
            return field;
        }
        private set;
    }

    protected abstract Vector2 GetResolution();

    public void UpdateResolution()
    {
        _lastGameResolution = Engine.ViewPort.InternalGameResolution;

        Vector2 prevResolution = _resolution;

        // Get the new resolution
        Resolution = GetResolution();
        
        // Update the scale if the resolution has changed
        if (_resolution != prevResolution)
            UpdateScale();
    }

    private void UpdateScale()
    {
        _lastViewPortRenderBox = Engine.ViewPort.RenderBox;
        _lastViewPortFullSize = Engine.ViewPort.FullSize;

        // Get the view port render box. This is the area on the screen we want to draw to.
        Vector2 viewPortRenderBoxSize;
        Vector2 viewPortRenderBoxPos;
        if (FitToGameViewPort)
        {
            viewPortRenderBoxSize = Engine.ViewPort.RenderBox.Size;
            viewPortRenderBoxPos = Engine.ViewPort.RenderBox.Position;
        }
        else
        {
            viewPortRenderBoxSize = Engine.ViewPort.FullSize;
            viewPortRenderBoxPos = Vector2.Zero;
        }

        // Get the resolution
        Vector2 res = Resolution;

        // Get the aspect ratios
        float screenRatio = viewPortRenderBoxSize.X / viewPortRenderBoxSize.Y;
        float gameRatio = res.X / res.Y;

        // Calculate the scale, size and position
        float scale;
        Vector2 size;
        Vector2 pos = Vector2.Zero;
        if (screenRatio > gameRatio)
        {
            scale = viewPortRenderBoxSize.Y / res.Y;
            size = res * scale;

            switch (HorizontalAlignment)
            {
                default:
                case HorizontalAlignment.Left:
                    // Do nothing
                    break;
                
                case HorizontalAlignment.Center:
                    pos = new Vector2((viewPortRenderBoxSize.X - size.X) / 2, 0);
                    break;
                
                case HorizontalAlignment.Right:
                    pos = new Vector2(viewPortRenderBoxSize.X - size.X, 0);
                    break;
            }
        }
        else
        {
            scale = viewPortRenderBoxSize.X / res.X;
            size = res * scale;

            switch (VerticalAlignment)
            {
                default:
                case VerticalAlignment.Top:
                    // Do nothing
                    break;

                case VerticalAlignment.Center:
                    pos = new Vector2(0, (viewPortRenderBoxSize.Y - size.Y) / 2);
                    break;
                
                case VerticalAlignment.Bottom:
                    pos = new Vector2(0, viewPortRenderBoxSize.Y - size.Y);
                    break;
            }
        }

        // Offset by the view port render box position
        pos += viewPortRenderBoxPos;

        // Set the scale
        Scale = scale;

        Viewport = new Viewport(new Rectangle(
            location: pos.ToCeilingPoint(),
            size: (res * scale).ToFloorPoint()));
    }

    public Vector2 ToWorldPosition(Vector2 pos)
    {
        pos.X -= Viewport.X;
        pos.Y -= Viewport.Y;
        pos /= Scale;
        return pos;
    }
    public Vector2 ToScreenPosition(Vector2 pos)
    {
        pos *= Scale;
        pos.X += Viewport.X;
        pos.Y += Viewport.Y;
        return pos;
    }

    public virtual void Update()
    {
        if (Engine.ViewPort.InternalGameResolution != _lastGameResolution)
            UpdateResolution();
        
        if (FitToGameViewPort && Engine.ViewPort.RenderBox != _lastViewPortRenderBox)
            UpdateScale();
        else if (!FitToGameViewPort && Engine.ViewPort.FullSize != _lastViewPortFullSize)
            UpdateScale();
    }
}