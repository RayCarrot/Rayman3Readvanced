using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class GbaRenderTarget
{
    public GbaRenderTarget(GraphicsDevice graphicsDevice, SurfaceFormat surfaceFormat, DepthFormat depthFormat)
    {
        GraphicsDevice = graphicsDevice;
        SurfaceFormat = surfaceFormat;
        DepthFormat = depthFormat;
    }

    private Point? _pendingResize;
    private RenderTargetBinding[] _prevRenderTargets;

    public GraphicsDevice GraphicsDevice { get; }
    public SurfaceFormat SurfaceFormat { get; }
    public DepthFormat DepthFormat { get; }
    public Point Size { get; private set; }
    public RenderTarget2D RenderTarget { get; private set; }

    public void SetSize(Point newSize)
    {
        if (newSize != Size)
        {
            // Save resizing for next render so we don't create a black texture during this frame
            _pendingResize = newSize;
        }
    }

    public void BeginRender()
    {
        if (_pendingResize != null)
        {
            RenderTarget?.Dispose();
            RenderTarget = new RenderTarget2D(
                GraphicsDevice,
                // Make sure the size doesn't reach 0 during resizing
                Math.Max(_pendingResize.Value.X, 1),
                Math.Max(_pendingResize.Value.Y, 1),
                false,
                SurfaceFormat,
                DepthFormat);

            Size = _pendingResize.Value;
            _pendingResize = null;
        }

        _prevRenderTargets = GraphicsDevice.GetRenderTargets();
        GraphicsDevice.SetRenderTarget(RenderTarget);
    }

    public void EndRender()
    {
        GraphicsDevice.SetRenderTargets(_prevRenderTargets);
    }
}