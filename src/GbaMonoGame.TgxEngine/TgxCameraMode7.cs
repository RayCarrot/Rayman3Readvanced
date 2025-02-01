using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

// TODO: Scale by the resolution. Currently the scaling is overriden by the shader.
public class TgxCameraMode7 : TgxCamera
{
    public TgxCameraMode7(RenderContext renderContext) : base(renderContext)
    {
        // TODO: Dispose
        BasicEffectShader = new BasicEffect(Engine.GraphicsDevice)
        {
            World = Matrix.Identity,
            TextureEnabled = true,
            VertexColorEnabled = true,
        };
    }

    private bool _isProjectionDirty = true;
    private bool _isViewDirty = true;
    private Vector2 _prevResolution = Vector2.Zero;

    private float _cameraFieldOfView = MathHelper.PiOver4;
    private float _cameraDistance = 100.0f;
    private float _cameraHeight = 50.0f;
    private float _cameraFar = 300.0f;
    private float _cameraTargetHeight = 10.0f;
    private Vector2 _position;
    private float _direction;

    // Rendering
    public BasicEffect BasicEffectShader { get; }

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

    public Vector2 GetDirection(int offset = 0)
    {
        return new Vector2(MathHelpers.Cos256(Direction + offset), MathHelpers.Sin256(Direction + offset));
    }

    public Vector2 GetRenderPosition2D()
    {
        // Get the direction and offset by 180 degrees
        Vector2 dir = GetDirection(128);

        // Multiply by the distance
        Vector2 cameraOffset = dir * CameraDistance;

        // Add to the position
        return Position + cameraOffset;
    }

    public Vector3 GetRenderPosition3D()
    {
        // Get the direction and offset by 180 degrees
        Vector2 dir = GetDirection(128);

        // Multiply by the distance and define the height
        Vector3 cameraOffset = new(
            value: dir * CameraDistance,
            z: -CameraHeight);

        // Add to the position
        return new Vector3(Position, 0) + cameraOffset;
    }

    public void Step()
    {
        // Get the current resolution
        Vector2 res = RenderContext.Resolution;

        // Update projection
        if (_isProjectionDirty || _prevResolution != res)
        {
            // Set the projection
            BasicEffectShader.Projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView: CameraFieldOfView,
                aspectRatio: RenderContext.AspectRatio,
                nearPlaneDistance: 0.1f,
                farPlaneDistance: CameraFar);

            _isProjectionDirty = false;
            _prevResolution = res;
        }

        // Update view
        if (_isViewDirty)
        {
            // Get the position
            Vector3 cameraPosition = GetRenderPosition3D();
            Vector3 cameraTarget = new(Position.X, Position.Y, -CameraTargetHeight);
            Vector3 cameraUp = new(0, 0, -1);

            // Set the view
            BasicEffectShader.View = Matrix.CreateLookAt(
                cameraPosition: cameraPosition,
                cameraTarget: cameraTarget,
                cameraUpVector: cameraUp);

            _isViewDirty = false;
        }
    }
}