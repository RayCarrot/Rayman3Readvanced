using System.Linq;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class ParallaxClusterRenderContext : RenderContext
{
    public ParallaxClusterRenderContext(Playfield2DRenderContext parentRenderContext, TgxCluster cluster)
    {
        ParentRenderContext = parentRenderContext;
        Cluster = cluster;
    }

    private Vector2 _lastParentResolution;

    public Playfield2DRenderContext ParentRenderContext { get; }
    public TgxCluster Cluster { get; }

    protected override Vector2 GetResolution()
    {
        _lastParentResolution = ParentRenderContext.Resolution;

        // We want the parallax backgrounds to target the original resolution since that
        // most closely matches how they were meant to be rendered. But since you can
        // play in higher internal resolution (like for widescreen) we have to make
        // sure it doesn't exceed the size of the smallest tile layer.
        Vector2 originalResolution = Rom.OriginalResolution;
        float ratio = ParentRenderContext.Resolution.X / ParentRenderContext.Resolution.Y;

        Vector2 ogRes;
        if (ratio > 1)
            ogRes = new Vector2(ratio * originalResolution.Y, originalResolution.Y);
        else
            ogRes = new Vector2(originalResolution.X, 1 / ratio * originalResolution.X);

        Vector2 res = ogRes;

        if (res == Rom.OriginalResolution)
            return res;

        float maxX = Cluster.Layers.Min(x => x.PixelWidth);
        float maxY = Cluster.Layers.Min(x => x.PixelHeight);

        if (res.X > res.Y)
        {
            if (res.Y > maxY)
                res = new Vector2(maxY * res.X / res.Y, maxY);

            if (res.X > maxX)
                res = new Vector2(maxX, maxX * res.Y / res.X);
        }
        else
        {
            if (res.X > maxX)
                res = new Vector2(maxX, maxX * res.Y / res.X);

            if (res.Y > maxY)
                res = new Vector2(maxY * res.X / res.Y, maxY);
        }

        if (res != ogRes)
            Logger.Debug("Set cluster with size {0} render context resolution to {1}", Cluster.Size, res);

        return res;
    }

    public override void Update()
    {
        base.Update();
    
        if (_lastParentResolution != ParentRenderContext.Resolution)
            UpdateResolution();
    }
}