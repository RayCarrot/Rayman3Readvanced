using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class TgxCamera2D : TgxCamera
{
    public TgxCamera2D(Playfield2DRenderContext renderContext) : base(renderContext) { }

    private TgxCluster _mainCluster;
    private readonly List<TgxCluster> _clusters = [];

    public new Playfield2DRenderContext RenderContext => (Playfield2DRenderContext)base.RenderContext;

    public override Vector2 Position
    {
        get => _mainCluster.Position;
        set
        {
            TgxCluster mainCluster = GetMainCluster();
            mainCluster.Position = value;

            foreach (TgxCluster cluster in _clusters)
            {
                if (cluster.Stationary)
                    continue;

                //Vector2 scrollFactor = cluster.GetMaxPosition(Rom.OriginalResolution) /
                //                       mainCluster.GetMaxPosition(Rom.OriginalResolution);
                Vector2 scrollFactor;

                // If it's scaled then we have to update the scroll factor
                if (cluster.RenderContext != RenderContext &&
                    RenderContext.Resolution != Rom.OriginalResolution)
                {
                    // Determine if the cluster wraps horizontally. We assume that none of them wrap vertically.
                    bool wrapX = cluster.Layers.Any(x => x.PixelWidth < cluster.Size.X);

                    if (wrapX)
                    {
                        // If the cluster wraps we use the original scroll factor (scaling it by the different resolutions)
                        scrollFactor = cluster.ScrollFactor * cluster.RenderContext.Resolution / RenderContext.Resolution;
                    }
                    else if (mainCluster.MaxPosition.X != 0)
                    {
                        // If the cluster does not wrap we want it to scroll evenly through the width of the level
                        scrollFactor = new Vector2(
                            cluster.GetMaxPosition(cluster.RenderContext.Resolution).X / mainCluster.MaxPosition.X,
                            cluster.ScrollFactor.Y);
                    }
                    else
                    {
                        scrollFactor = new Vector2(0, cluster.ScrollFactor.Y);
                    }
                }
                else
                {
                    scrollFactor = cluster.ScrollFactor;
                }

                cluster.Position = (mainCluster.Position - mainCluster.Origin) * scrollFactor;
            }
        }
    }

    public TgxCluster GetMainCluster() => GetCluster(0);

    public TgxCluster GetCluster(int clusterId)
    {
        if (clusterId == 0)
            return _mainCluster ?? throw new Exception("The main cluster hasn't been added yet");
        else
            return _clusters[clusterId - 1];
    }

    public IEnumerable<TgxCluster> GetClusters(bool includeMain)
    {
        if (includeMain)
            yield return GetMainCluster();

        foreach (TgxCluster cluster in _clusters)
            yield return cluster;
    }

    public void AddCluster(ClusterResource clusterResource)
    {
        if (_mainCluster == null)
            _mainCluster = new TgxCluster(clusterResource, RenderContext);
        else
            _clusters.Add(new TgxCluster(clusterResource, RenderContext));
    }

    public void AddLayer(int clusterId, TgxGameLayer layer)
    {
        TgxCluster cluster = GetCluster(clusterId);
        cluster.AddLayer(layer);
    }
}