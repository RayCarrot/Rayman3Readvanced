﻿using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class MenuScrollBar
{
    public MenuScrollBar(RenderContext renderContext, Vector2 position, int bgPriority)
    {
        Texture2D scrollBarThumbTexture = Engine.FixContentManager.Load<Texture2D>(Assets.ScrollBarThumbTexture);

        Position = position;

        ScrollBar = new SpriteTextureObject
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            RenderContext = renderContext,
        };

        ScrollBarThumb = new SpriteTextureObject
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            Texture = scrollBarThumbTexture,
            RenderContext = renderContext,
        };

        Size = MenuScrollBarSize.Big;
    }

    private const float ThumbOffsetX = 5;
    private const float ThumbOffsetY = 16;

    private MenuScrollBarSize _size;

    public MenuScrollBarSize Size
    {
        get => _size;
        set
        {
            if (value == _size && ScrollBar.Texture != null)
                return;

            _size = value;

            ScrollBar.Texture = Engine.FixContentManager.Load<Texture2D>(value switch
            {
                MenuScrollBarSize.Small => Assets.ScrollBarSmallTexture,
                MenuScrollBarSize.Big => Assets.ScrollBarBigTexture,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });
        }
    }

    public Vector2 Position { get; set; }

    public SpriteTextureObject ScrollBar { get; }
    public SpriteTextureObject ScrollBarThumb { get; }

    public float ScrollOffset { get; set; }
    public float MaxScrollOffset { get; set; }

    private float GetLength()
    {
        return Size switch
        {
            MenuScrollBarSize.Small => 32,
            MenuScrollBarSize.Big => 87,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        ScrollBar.ScreenPos = Position;
        animationPlayer.Play(ScrollBar);

        if (MaxScrollOffset != 0)
        {
            float length = GetLength();
            float scrollY = MathHelper.Lerp(0, length, ScrollOffset / MaxScrollOffset);
            ScrollBarThumb.ScreenPos = Position + new Vector2(ThumbOffsetX, ThumbOffsetY + scrollY);
            animationPlayer.Play(ScrollBarThumb);
        }
    }
}