using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public abstract class RenderContext
{
    private Box _lastViewPortRenderBox;
    private Vector2 _lastGameResolution;

    private Vector2 _resolution;
    private float _scale;
    private Viewport _viewPort;

    protected virtual HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Center;
    protected virtual VerticalAlignment VerticalAlignment => VerticalAlignment.Center;

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
            return _scale;
        }
        private set => _scale = value;
    }

    public Viewport Viewport
    {
        get
        {
            Update();
            return _viewPort;
        }
        private set => _viewPort = value;
    }

    protected abstract Vector2 GetResolution();

    public void UpdateResolution()
    {
        _lastGameResolution = Engine.Config.InternalGameResolution;

        Vector2 prevResolution = _resolution;

        // Get the new resolution
        Resolution = GetResolution();
        
        // Update the scale if the resolution has changed
        if (_resolution != prevResolution)
            UpdateScale();
    }

    private void UpdateScale()
    {
        _lastViewPortRenderBox = Engine.GameViewPort.RenderBox;

        // Get the resolution
        Vector2 res = Resolution;

        // Get the view port render box. This is the area on the screen we want to draw to.
        Box viewPortRenderBox = Engine.GameViewPort.RenderBox;
        Vector2 viewPortSize = viewPortRenderBox.Size;

        // Get the aspect ratios
        float screenRatio = viewPortSize.X / viewPortSize.Y;
        float gameRatio = res.X / res.Y;

        // Calculate the scale, size and position
        float scale;
        Vector2 size;
        Vector2 pos = Vector2.Zero;
        if (screenRatio > gameRatio)
        {
            scale = viewPortSize.Y / res.Y;
            size = res * scale;

            switch (HorizontalAlignment)
            {
                default:
                case HorizontalAlignment.Left:
                    // Do nothing
                    break;
                
                case HorizontalAlignment.Center:
                    pos = new Vector2((viewPortSize.X - size.X) / 2, 0);
                    break;
                
                case HorizontalAlignment.Right:
                    pos = new Vector2(viewPortSize.X - size.X, 0);
                    break;
            }
        }
        else
        {
            scale = viewPortSize.X / res.X;
            size = res * scale;

            switch (VerticalAlignment)
            {
                default:
                case VerticalAlignment.Top:
                    // Do nothing
                    break;

                case VerticalAlignment.Center:
                    pos = new Vector2(0, (viewPortSize.Y - size.Y) / 2);
                    break;
                
                case VerticalAlignment.Bottom:
                    pos = new Vector2(0, viewPortSize.Y - size.Y);
                    break;
            }
        }

        // Offset by the view port render box position
        pos += viewPortRenderBox.Position;

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
        if (Engine.Config.InternalGameResolution != _lastGameResolution)
            UpdateResolution();
        
        if (Engine.GameViewPort.RenderBox != _lastViewPortRenderBox)
            UpdateScale();
    }
}