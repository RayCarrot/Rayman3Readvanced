using System.Linq;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class ParallaxClusterRenderContext : RenderContext
{
    public ParallaxClusterRenderContext(TgxCluster cluster)
    {
        Cluster = cluster;
    }

    public TgxCluster Cluster { get; }

    protected override Vector2 GetResolution()
    {
        // We want the parallax backgrounds to target the screen resolution since that
        // most closely matches how they were meant to be rendered. But since you can
        // play in higher internal resolution (like for widescreen) we have to make
        // sure it doesn't exceed the size of the smallest tile layer.
        Vector2 ogRes = Rom.OriginalGameRenderContext.Resolution;
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
}