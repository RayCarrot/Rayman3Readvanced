using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class TgxCluster
{
    public TgxCluster(ClusterResource cluster, Playfield2DRenderContext renderContext)
    {
        ScrollFactor = new Vector2(cluster.ScrollFactor.X, cluster.ScrollFactor.Y);
        Layers = new List<TgxGameLayer>();
        Size = new Vector2(cluster.SizeX * Tile.Size, cluster.SizeY * Tile.Size);
        Stationary = cluster.Stationary;

        // Render parallax backgrounds using a separate render context so we can scale them
        RenderContext = !Stationary && ScrollFactor == Vector2.One 
            ? renderContext
            : new ParallaxClusterRenderContext(renderContext, this);
    }

    private Vector2 _position;

    public List<TgxGameLayer> Layers { get; }

    public Vector2 Size { get; set; }

    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = Vector2.Clamp(value, Vector2.Zero, MaxPosition);

            foreach (TgxGameLayer layer in Layers)
                layer.SetOffset(_position);
        }
    }

    public Vector2 MaxPosition => GetMaxPosition(RenderContext.Resolution);

    public Vector2 ScrollFactor { get; set; }

    public bool Stationary { get; }
    public RenderContext RenderContext { get; }

    public Vector2 GetMaxPosition(Vector2 resolution) => new(Math.Max(0, Size.X - resolution.X), Math.Max(0, Size.Y - resolution.Y));

    public void AddLayer(TgxGameLayer layer)
    {
        Layers.Add(layer);
    }
    
    public bool IsOnLimit(Edge limit)
    {
        Vector2 maxPos = MaxPosition;

        return limit switch
        {
            // In the game these are == checks, but since we're dealing with floats here they're <= and >=
            Edge.Top => Position.Y <= 0,
            Edge.Right => Position.X >= maxPos.X,
            Edge.Bottom => Position.Y >= maxPos.Y,
            Edge.Left => Position.X <= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(limit), limit, null)
        };
    }
}