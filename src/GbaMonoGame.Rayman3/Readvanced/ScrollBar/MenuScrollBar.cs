using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class MenuScrollBar
{
    public MenuScrollBar(RenderContext renderContext, Vector2 position, int bgPriority)
    {
        Texture2D scrollBarThumbTexture = Engine.Assets.FixContentManager.Load<Texture2D>(Assets.Menu.ScrollBarThumb);

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

    public MenuScrollBarSize Size
    {
        get;
        set
        {
            if (value == field && ScrollBar.Texture != null)
                return;

            field = value;

            ScrollBar.Texture = Engine.Assets.FixContentManager.Load<Texture2D>(value switch
            {
                MenuScrollBarSize.Small => Assets.Menu.ScrollBarSmall,
                MenuScrollBarSize.Big => Assets.Menu.ScrollBarBig,
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