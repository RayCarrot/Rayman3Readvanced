using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class SpriteTextureObject : AObject
{
    public Texture2D Texture { get; set; }
    public AffineMatrix? AffineMatrix { get; set; }

    public override void Execute(Action<short> soundEventCallback)
    {
        Gfx.AddSprite(new Sprite
        {
            Texture = Texture,
            Position = GetAnchoredPosition(),
            AffineMatrix = AffineMatrix,
            Center = true,
            Priority = BgPriority,
            RenderOptions = RenderOptions,
        });
    }
}