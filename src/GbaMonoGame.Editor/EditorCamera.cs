using System;
using System.Collections.Generic;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Editor;

public class EditorCamera
{
    public EditorCamera(Vector2 mapSize)
    {
        ScrollBounds = new Box(
            minX: 0 - ScrollMargin,
            minY: 0 - ScrollMargin,
            maxX: mapSize.X + ScrollMargin,
            maxY: mapSize.Y + ScrollMargin);
        RenderContext = new EditorRenderContext
        {
            Scale = Scale,
            MaxResolution = ScrollBounds.Size
        };

        GameLayers = new List<TgxGameLayer>();
    }

    private const int ScrollMargin = Tile.Size * 10;
    private const float DefaultScale = 1.5f;
    private const float MouseWheelZoomSpeed = 0.05f;

    private Vector2 _position;

    public Box ScrollBounds { get; }
    public EditorRenderContext RenderContext { get; }
    public Vector2 Position
    {
        get => _position;
        set
        {
            Vector2 minPos = ScrollBounds.Position;
            Vector2 maxPos = new(Math.Max(minPos.X, ScrollBounds.MaxX - RenderContext.Resolution.X), Math.Max(minPos.Y, ScrollBounds.MaxY - RenderContext.Resolution.Y));

            _position = Vector2.Clamp(value, minPos, maxPos);

            foreach (TgxGameLayer gameLayer in GameLayers)
                gameLayer.SetOffset(_position);
        }
    }
    public List<TgxGameLayer> GameLayers { get; }

    public float Scale { get; set; } = DefaultScale;

    public bool IsActorFramed(EditableActor actor)
    {
        Box viewBox = actor.GetViewBox();
        Box camBox = new(Position, RenderContext.Resolution);

        bool isFramed = viewBox.Intersects(camBox);

        if (isFramed)
            actor.AnimatedObject.ScreenPos = actor.Position - Position;

        return isFramed;
    }

    public void AddGameLayer(TgxGameLayer gameLayer)
    {
        GameLayers.Add(gameLayer);
    }

    public void Step()
    {
        // Zoom
        int wheelDelta = InputManager.GetMouseWheelDelta();
        if (wheelDelta < 0)
        {
            Scale += MouseWheelZoomSpeed;
            RenderContext.Scale = Scale;
            RenderContext.UpdateResolution();
        }
        else if (wheelDelta > 0)
        {
            Scale -= MouseWheelZoomSpeed;
            RenderContext.Scale = Scale;
            RenderContext.UpdateResolution();
        }

        // Scroll
        if (InputManager.GetMouseState().RightButton == ButtonState.Pressed)
            Position += InputManager.GetMousePositionDelta(RenderContext) * -1;
    }
}