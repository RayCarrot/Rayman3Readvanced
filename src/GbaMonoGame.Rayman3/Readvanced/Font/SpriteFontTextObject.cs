using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.Readvanced;

public class SpriteFontTextObject : AObject
{
    public string Text { get; set; }

    public Font Font { get; set; }
    public AffineMatrix? AffineMatrix { get; set; }
    public float Alpha { get; set; } = 1;

    public override void Execute(Action<short> soundEventCallback)
    {
        if (Text == null)
            return;

        if (Font == null)
            throw new Exception("Can't render text without a font");

        Vector2 originalPos = GetAnchoredPosition();

        Matrix transformation = Matrix.Identity;
        transformation.M11 = AffineMatrix?.Scale.X ?? 1;
        transformation.M22 = AffineMatrix?.Scale.Y ?? 1;
        transformation.M41 = originalPos.X;
        transformation.M42 = originalPos.Y;

        Vector2 pos = Vector2.Zero;

        for (int i = 0; i < Text.Length; i++)
        {
            char c = Text[i];

            if (c == '\r')
                continue;

            // Linebreak
            if (c == '\n')
            {
                pos.X = 0;
                pos.Y += Font.LineHeight;
                continue;
            }

            Sprite sprite = Font.GetCharacterSprite(
                text: Text,
                charIndex: i,
                transformation: transformation,
                position: ref pos,
                priority: BgPriority,
                affineMatrix: AffineMatrix,
                alpha: Alpha,
                color: Color.White,
                renderOptions: RenderOptions);

            if (sprite != null)
                Gfx.AddSprite(sprite);
        }
    }
}