using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

// TODO: Scale by the resolution. Currently the scaling is overriden by the shader.
public class TgxCameraMode7 : TgxCamera
{
    public TgxCameraMode7(RenderContext renderContext) : base(renderContext)
    {
        BasicEffectShader = new BasicEffect(Engine.GraphicsDevice)
        {
            World = Matrix.Identity,
            TextureEnabled = true,
            VertexColorEnabled = true,
        };
        CachedBasicEffectShaders = new Dictionary<object, BasicEffect>();
    }

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
    public BasicEffect BasicEffectShader { get; }
    public Dictionary<object, BasicEffect> CachedBasicEffectShaders { get; }

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

    private static bool WithinEpsilon(float a, float b)
    {
        float num = a - b;
        return num is >= -1.401298E-45f and <= Single.Epsilon;
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
        // Re-implemented from ViewPort.Project in MonoGame

        Vector2 res = RenderContext.Resolution;

        Matrix matrix = BasicEffectShader.World * BasicEffectShader.View * BasicEffectShader.Projection;
        Vector3 vector = Vector3.Transform(source, matrix);

        float a = source.X * matrix.M14 + source.Y * matrix.M24 + source.Z * matrix.M34 + matrix.M44;
        if (!WithinEpsilon(a, 1f))
        {
            vector.X /= a;
            vector.Y /= a;
            vector.Z /= a;
        }

        // Scale by the resolution
        vector.X = (((vector.X + 1f) * 0.5f) * res.X);
        vector.Y = (((-vector.Y + 1f) * 0.5f) * res.Y);

        return vector;
    }

    public override void UnInit()
    {
        base.UnInit();

        BasicEffectShader.Dispose();

        foreach (BasicEffect effect in CachedBasicEffectShaders.Values)
            effect.Dispose();
        CachedBasicEffectShaders.Clear();
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
            // Set the view
            BasicEffectShader.View = Matrix.CreateLookAt(
                cameraPosition: new Vector3(Position.X, Position.Y, -CameraHeight),
                cameraTarget: GetCameraTarget(),
                cameraUpVector: new Vector3(0, 0, -1));

            _isViewDirty = false;
        }
    }
}