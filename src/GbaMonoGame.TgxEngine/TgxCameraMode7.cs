using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

// TODO: Scale by the resolution. Currently the scaling is overriden by the shader.
public class TgxCameraMode7 : TgxCamera
{
    public TgxCameraMode7(RenderContext renderContext) : base(renderContext)
    {
        TextLayerRenderContext = new TextLayerRenderContext(renderContext);
        Horizon = DefaultHorizon;
        Step();
    }

    private const float DefaultHorizon = 62;

    private bool _isProjectionDirty = true;
    private bool _isViewDirty = true;
    private bool _isCameraFarDirty = true;
    private Vector2 _prevResolution = Vector2.Zero;

    private float _cameraFieldOfView = MathHelper.PiOver4;
    private float _cameraDistance = 790.0f;
    private float _cameraTargetHeight = -85.0f;
    private float _cameraFar = 0.2f;
    private float _cameraHeight = 20.0f;
    private Vector2 _position;
    private Angle256 _direction;
    private float _horizon;

    // Rendering
    public Matrix ViewMatrix { get; set; }
    public Matrix ProjectionMatrix { get; set; }
    public Matrix ViewProjectionMatrix { get; set; }

    // Projection values
    public float CameraFieldOfView
    {
        get => _cameraFieldOfView;
        set
        {
            if (_cameraFieldOfView != value)
            {
                _cameraFieldOfView = value;
                _isProjectionDirty = true;
                _isCameraFarDirty = true;
            }
        }
    }
    public float CameraFar
    {
        get => _cameraFar;
        set
        {
            if (_cameraFar != value)
            {
                _cameraFar = value;
                _isProjectionDirty = true;
            }
        }
    }

    // View values
    public float CameraDistance
    {
        get => _cameraDistance;
        set
        {
            if (_cameraDistance != value)
            {
                _cameraDistance = value;
                _isViewDirty = true;
                _isCameraFarDirty = true;
            }
        }
    }
    public float CameraHeight
    {
        get => _cameraHeight;
        set
        {
            if (_cameraHeight != value)
            {
                _cameraHeight = value;
                _isViewDirty = true;
                _isCameraFarDirty = true;
            }
        }
    }
    public float CameraTargetHeight
    {
        get => _cameraTargetHeight;
        set
        {
            if (_cameraTargetHeight != value)
            {
                _cameraTargetHeight = value;
                _isViewDirty = true;
                _isCameraFarDirty = true;
            }
        }
    }

    // Positioning
    public override Vector2 Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                _isViewDirty = true;
            }
        }
    }
    public Angle256 Direction
    {
        get => _direction;
        set
        {
            if (_direction != value)
            {
                _direction = value;
                _isViewDirty = true;

                // Update text layers
                foreach (TgxTextLayerMode7 layer in TextLayers)
                {
                    if (!layer.IsStatic)
                        layer.ScrolledPosition = layer.ScrolledPosition with { X = layer.RotationFactor * Direction };
                }
            }
        }
    }

    // Horizon
    public float Horizon
    {
        get => _horizon;
        set
        {
            if (_horizon != value)
            {
                _horizon = value;
                _isCameraFarDirty = true;
            }
        }
    }

    public TextLayerRenderContext TextLayerRenderContext { get; }

    // Layers
    public List<TgxGameLayer> RotScaleLayers { get; } = new();
    public List<TgxTextLayerMode7> TextLayers { get; } = new();

    private void UpdateProjectionMatrix()
    {
        // Set the projection matrix
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            fieldOfView: CameraFieldOfView,
            aspectRatio: RenderContext.AspectRatio,
            nearPlaneDistance: 0.1f,
            farPlaneDistance: CameraFar);

        // If we changed the horizon then we have to vertically shift the map. The game uses 0 as the base, but we
        // use the default horizon value as the base instead since that's what the camera values are all based on.
        float verticalShift = DefaultHorizon - Horizon;

        // Convert from screen space to clip space (-1 to 1)
        float shiftInClipSpace = 2 * verticalShift / RenderContext.Resolution.Y;

        // Create a translation matrix for the shift
        Matrix screenShift = Matrix.CreateTranslation(0, shiftInClipSpace, 0);

        // Shift the projection matrix
        ProjectionMatrix *= screenShift;
    }

    private void UpdateViewMatrix()
    {
        // Set the view
        ViewMatrix = Matrix.CreateLookAt(
            cameraPosition: new Vector3(Position.X, Position.Y, -CameraHeight),
            cameraTarget: GetCameraTarget(),
            cameraUpVector: new Vector3(0, 0, -1));
    }

    public void AddRotScaleLayer(TgxGameLayer layer)
    {
        RotScaleLayers.Add(layer);
    }

    public void AddTextLayer(TgxTextLayerMode7 layer)
    {
        TextLayers.Add(layer);
    }

    public Vector3 GetCameraTarget()
    {
        // Get the direction
        Vector2 dir = Direction.ToDirectionalVector();

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
        float scale = RenderContext.Scale;

        if (inViewPort)
        {
            source *= scale;
            source += new Vector2(viewport.X, viewport.Y);
        }

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

        bool updateViewProj = false;

        // Update resolution
        if (_prevResolution != res)
        {
            _prevResolution = res;

            // If the resolution is changes then we have to update the projection
            _isProjectionDirty = true;
        }

        // Update projection
        if (_isProjectionDirty || _isCameraFarDirty)
        {
            _isProjectionDirty = false;
            
            // Update the matrix
            UpdateProjectionMatrix();
            
            // Flag to update the ViewProj matrix
            updateViewProj = true;
        }

        // Update view
        if (_isViewDirty)
        {
            _isViewDirty = false;

            // Update the matrix
            UpdateViewMatrix();

            // Flag to update the ViewProj matrix
            updateViewProj = true;
        }

        // Update camera far
        if (_isCameraFarDirty)
        {
            _isCameraFarDirty = false;

            // Update the horizon
            TextLayerRenderContext.Horizon = Horizon;
            TextLayerRenderContext.UpdateResolution();

            // NOTE: It should be horizon+1, but there can be slight scaling artifacts, so better doing two pixels behind the background to ensure there's no empty space in-between
            Vector3 world = Unproject(new Vector2(res.X / 2, Horizon - 1), true);

            if (world != Vector3.Zero)
            {
                Vector3 camPos = new(Position.X, Position.Y, -CameraHeight);
                float dist = Vector3.Distance(camPos, world);
                if (dist != _cameraFar)
                {
                    _cameraFar = dist;
                    UpdateProjectionMatrix();
                    updateViewProj = true;
                }
            }
            // Fallback if world is zero, which would usually happen if the horizon is too low compared to the resolution, so we see waaay past the map
            else if (RotScaleLayers.Count > 0)
            {
                float dist = RotScaleLayers[0].PixelWidth;
                if (dist != _cameraFar)
                {
                    _cameraFar = dist;
                    UpdateProjectionMatrix();
                    updateViewProj = true;
                }
            }
        }

        if (updateViewProj)
        {
            // Update the ViewProj matrix
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;

            // Update rotscale layers
            foreach (TgxGameLayer layer in RotScaleLayers)
                layer.SetWorldViewProjMatrix(ViewProjectionMatrix);
        }
    }
}