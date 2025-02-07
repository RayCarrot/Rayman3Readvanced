using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

// TODO: Scale by the resolution. Currently the scaling is overriden by the shader.
public class TgxCameraMode7 : TgxCamera
{
    public TgxCameraMode7(RenderContext renderContext) : base(renderContext) { }

    private bool _isProjectionDirty = true;
    private bool _isViewDirty = true;
    private Vector2 _prevResolution = Vector2.Zero;

    private float _cameraFieldOfView = MathHelper.PiOver4;
    private float _cameraDistance = 250.0f;
    private float _cameraTargetHeight = -40.0f;
    private float _cameraFar = 386.0f;
    private float _cameraHeight = 22.0f;
    private Vector2 _position;
    private float _direction;

    // Rendering
    public Matrix ViewMatrix { get; set; }
    public Matrix ProjectionMatrix { get; set; }

    // Projection values
    public float CameraFieldOfView
    {
        get => _cameraFieldOfView;
        set => SetProjectionValue(ref _cameraFieldOfView, value);
    }
    public float CameraFar
    {
        get => _cameraFar;
        set => SetProjectionValue(ref _cameraFar, value);
    }

    // View values
    public float CameraDistance
    {
        get => _cameraDistance;
        set => SetViewValue(ref _cameraDistance, value);
    }
    public float CameraHeight
    {
        get => _cameraHeight;
        set => SetViewValue(ref _cameraHeight, value);
    }
    public float CameraTargetHeight
    {
        get => _cameraTargetHeight;
        set => SetViewValue(ref _cameraTargetHeight, value);
    }

    // Positioning
    public override Vector2 Position
    {
        get => _position;
        set => SetViewValue(ref _position, value);
    }
    public float Direction
    {
        get => _direction;
        set => SetViewValue(ref _direction, value);
    }

    // Layers
    public List<TgxGameLayer> RotScaleLayers { get; } = new();

    private void SetProjectionValue<T>(ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
            return;

        field = newValue;
        _isProjectionDirty = true;
    }

    private void SetViewValue<T>(ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
            return;

        field = newValue;
        _isViewDirty = true;
    }

    public void AddRotScaleLayer(TgxGameLayer layer)
    {
        RotScaleLayers.Add(layer);
    }

    public Vector2 GetDirection()
    {
        return new Vector2(MathHelpers.Cos256(Direction), MathHelpers.Sin256(Direction));
    }

    public Vector3 GetCameraTarget()
    {
        // Get the direction
        Vector2 dir = GetDirection();

        // Multiply by the distance and set the height
        Vector3 cameraOffset = new(
            value: dir * CameraDistance,
            z: -CameraTargetHeight);

        // Add to the position
        return new Vector3(Position, 0) + cameraOffset;
    }

    public Vector3 Project(Vector3 source)
    {
        Viewport viewport = RenderContext.Viewport;
        float scale = RenderContext.Scale;

        // Project to the screen using the viewport
        Vector3 pos = viewport.Project(source, ProjectionMatrix, ViewMatrix, Matrix.Identity);
        
        // Offset by the viewport position on the screen
        pos -= new Vector3(viewport.X, viewport.Y, 0);

        // Scale
        pos /= scale;
        
        return pos;
    }

    public Vector3 Unproject(Vector2 source, bool inViewPort)
    {
        Viewport viewport = RenderContext.Viewport;

        if (inViewPort)
            source += new Vector2(viewport.X, viewport.Y);

        Vector3 nearWorldPoint = viewport.Unproject(new Vector3(source, 0), ProjectionMatrix, ViewMatrix, Matrix.Identity);
        Vector3 farWorldPoint = viewport.Unproject(new Vector3(source, 1), ProjectionMatrix, ViewMatrix, Matrix.Identity);
        Ray ray = new(nearWorldPoint, farWorldPoint - nearWorldPoint);

        Plane groundPlane = new(Vector3.Zero, Vector3.UnitZ);
        
        if (ray.Intersects(groundPlane) is { } distance)
            return ray.Position + distance * ray.Direction;
        else
            return Vector3.Zero;
    }

    public void Step()
    {
        // Get the current resolution
        Vector2 res = RenderContext.Resolution;

        bool updated = false;

        // Update projection
        if (_isProjectionDirty || _prevResolution != res)
        {
            // Set the projection
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView: CameraFieldOfView,
                aspectRatio: RenderContext.AspectRatio,
                nearPlaneDistance: 0.1f,
                farPlaneDistance: CameraFar);

            updated = true;
            _isProjectionDirty = false;
            _prevResolution = res;
        }

        // Update view
        if (_isViewDirty)
        {
            // Set the view
            ViewMatrix = Matrix.CreateLookAt(
                cameraPosition: new Vector3(Position.X, Position.Y, -CameraHeight),
                cameraTarget: GetCameraTarget(),
                cameraUpVector: new Vector3(0, 0, -1));

            updated = true;
            _isViewDirty = false;
        }

        if (updated)
        {
            Matrix viewProj = ViewMatrix * ProjectionMatrix;
            foreach (TgxGameLayer layer in RotScaleLayers)
                layer.SetWorldViewProjMatrix(viewProj);
        }
    }
}