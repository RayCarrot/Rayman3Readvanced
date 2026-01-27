using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class SpriteTextureObject : AObject
{
    public Texture2D Texture { get; set; }
    public AffineMatrix? AffineMatrix { get; set; }
    public AlphaCoefficient Alpha { get; set; }

    public override void Execute(Action<short> soundEventCallback)
    {
        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = Texture;
        sprite.Position = GetAnchoredPosition();
        sprite.AffineMatrix = AffineMatrix;
        sprite.Center = true;
        sprite.Priority = BgPriority;
        sprite.RenderOptions = RenderOptions;
        sprite.Alpha = Alpha;
        Gfx.AddSprite(sprite);
    }
}