using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public abstract class RenderContext
{
    private Vector2 _lastScreenSize;
    private Vector2 _lastGameResolution;

    private Vector2 _resolution;
    private Matrix _matrix;
    private Box _visibleArea;
    private Rectangle _viewPortRenderBox;

    public Vector2 Resolution
    {
        get
        {
            Update();
            return _resolution;
        }
        private set => _resolution = value;
    }

    public Matrix Matrix
    {
        get
        {
            Update();
            return _matrix;
        }
        private set => _matrix = value;
    }

    public Box VisibleArea
    {
        get
        {
            Update();
            return _visibleArea;
        }
        private set => _visibleArea = value;
    }

    public Rectangle ViewPortRenderBox
    {
        get
        {
            Update();
            return _viewPortRenderBox;
        }
        private set => _viewPortRenderBox = value;
    }

    protected abstract Vector2 GetResolution();

    public void UpdateResolution()
    {
        _lastGameResolution = Engine.Config.InternalGameResolution;

        Vector2 prevResolution = _resolution;

        // Get the new resolution
        Resolution = GetResolution();
        
        // Update the matrix if the resolution has changed
        if (_resolution != prevResolution)
            UpdateMatrix();
    }

    private void UpdateMatrix()
    {
        _lastScreenSize = Engine.GameViewPort.ScreenSize;

        // Get the resolution
        Vector2 res = Resolution;

        // Get a resolution extended to the internal game resolution's aspect ratio. For example, if we're
        // in a 3:2 area while playing in widescreen then this extends the resolution to the widescreen ratio.
        Vector2 scaledRes = res.ExtendToAspectRatio(Engine.Config.InternalGameResolution);

        // Get the view port screen size
        Vector2 screenSize = Engine.GameViewPort.ScreenSize;

        // Get the aspect ratios
        float screenRatio = screenSize.X / screenSize.Y;
        float gameRatio = scaledRes.X / scaledRes.Y;

        // Calculate the scale
        float scale;
        if (screenRatio > gameRatio)
            scale = screenSize.Y / scaledRes.Y;
        else
            scale = screenSize.X / scaledRes.X;

        // Scale the resolution to get the size to render to
        Vector2 size = res * scale;

        // Center the game
        Vector2 pos = new((screenSize.X - size.X) / 2, (screenSize.Y - size.Y) / 2);

        // Create a matrix
        Matrix matrix =
            // Scale
            Matrix.CreateScale(scale) * 
            // Center
            Matrix.CreateTranslation(pos.X, pos.Y, 0);

        // Set the matrix
        Matrix = matrix;

        ViewPortRenderBox = new Rectangle(
            location: pos.ToCeilingPoint(), 
            size: size.ToFloorPoint());

        Matrix inverseViewMatrix = Matrix.Invert(matrix);
        Vector2 tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
        Vector2 tr = Vector2.Transform(new Vector2(size.X, 0), inverseViewMatrix);
        Vector2 bl = Vector2.Transform(new Vector2(0, size.Y), inverseViewMatrix);
        Vector2 br = Vector2.Transform(size, inverseViewMatrix);
        Vector2 min = new(
            MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
            MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
        Vector2 max = new(
            MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
            MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
        VisibleArea = new Box(0, 0, max.X - min.X, max.Y - min.Y);
    }

    public Vector2 ToWorldPosition(Vector2 pos) => Vector2.Transform(pos, Matrix.Invert(Matrix));
    public Vector2 ToScreenPosition(Vector2 pos) => Vector2.Transform(pos, Matrix);

    public bool IsVisible(Box rect) => VisibleArea.Intersects(rect);
    public bool IsVisible(Vector2 position, Point size) => VisibleArea.Intersects(new Box(position.X, position.Y, position.X + size.X, position.Y + size.Y));
    public bool IsVisible(Vector2 position) => VisibleArea.Contains(position);

    public virtual void Update()
    {
        if (Engine.Config.InternalGameResolution != _lastGameResolution)
            UpdateResolution();
        
        if (Engine.GameViewPort.ScreenSize != _lastScreenSize)
            UpdateMatrix();
    }
}