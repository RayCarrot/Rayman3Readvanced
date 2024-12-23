using System;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public abstract class RenderContext
{
    protected RenderContext()
    {
        Engine.GameViewPort.GameResolutionChanged += GameViewPort_GameResolutionChanged;
        Engine.GameViewPort.Resized += GameViewPort_Resized;
    }

    private bool _hasSetResolution;
    private Vector2 _resolution;
    private Matrix _matrix;
    private Box _visibleArea;

    public Vector2 Resolution
    {
        get
        {
            if (!_hasSetResolution)
                UpdateResolution();

            return _resolution;
        }
        private set
        {
            _resolution = value;
            _hasSetResolution = true;
            Matrix = CreateRenderMatrix(Resolution);
        }
    }

    public Matrix Matrix
    {
        get
        {
            if (!_hasSetResolution)
                UpdateResolution();

            return _matrix;
        }
        private set
        {
            _matrix = value;
            VisibleArea = GetVisibleArea(Matrix);
        }
    }

    public Box VisibleArea
    {
        get
        {
            if (!_hasSetResolution)
                UpdateResolution();

            return _visibleArea;
        }
        private set => _visibleArea = value;
    }

    private void GameViewPort_GameResolutionChanged(object sender, EventArgs e) => 
        UpdateResolution();
    private void GameViewPort_Resized(object sender, EventArgs e) => 
        Matrix = CreateRenderMatrix(Resolution);

    private Matrix CreateRenderMatrix(Vector2 resolution)
    {
        float screenRatio = Engine.GameViewPort.ScreenSize.X / Engine.GameViewPort.ScreenSize.Y;
        float gameRatio = Resolution.X / Resolution.Y;

        float worldScale;

        if (screenRatio > gameRatio)
            worldScale = Engine.GameViewPort.ScreenSize.Y / resolution.Y;
        else
            worldScale = Engine.GameViewPort.ScreenSize.X / resolution.X;

        return Matrix.CreateScale(worldScale) *
               Matrix.CreateTranslation(Engine.GameViewPort.ScreenBox.MinX, Engine.GameViewPort.ScreenBox.MinY, 0);
    }

    private Box GetVisibleArea(Matrix matrix)
    {
        Matrix inverseViewMatrix = Matrix.Invert(matrix);
        Vector2 tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
        Vector2 tr = Vector2.Transform(new Vector2(Engine.GameViewPort.ScreenBox.Width, 0), inverseViewMatrix);
        Vector2 bl = Vector2.Transform(new Vector2(0, Engine.GameViewPort.ScreenBox.Height), inverseViewMatrix);
        Vector2 br = Vector2.Transform(Engine.GameViewPort.ScreenBox.Size, inverseViewMatrix);
        Vector2 min = new(
            MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
            MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
        Vector2 max = new(
            MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
            MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
        return new Box(0, 0, max.X - min.X, max.Y - min.Y);
    }

    protected abstract Vector2 GetResolution();

    public void UpdateResolution()
    {
        Resolution = GetResolution();
    }

    public Vector2 ToWorldPosition(Vector2 pos) => Vector2.Transform(pos, Matrix.Invert(Matrix));
    public Vector2 ToScreenPosition(Vector2 pos) => Vector2.Transform(pos, Matrix);

    public bool IsVisible(Box rect) => VisibleArea.Intersects(rect);
    public bool IsVisible(Vector2 position, Point size) => VisibleArea.Intersects(new Box(position.X, position.Y, position.X + size.X, position.Y + size.Y));
    public bool IsVisible(Vector2 position) => VisibleArea.Contains(position);

    public virtual void UnInit()
    {
        Engine.GameViewPort.GameResolutionChanged -= GameViewPort_GameResolutionChanged;
        Engine.GameViewPort.Resized -= GameViewPort_Resized;
    }
}