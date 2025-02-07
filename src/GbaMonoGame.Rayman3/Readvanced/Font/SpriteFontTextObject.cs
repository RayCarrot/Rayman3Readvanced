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
        Vector2 pos = originalPos;

        for (int i = 0; i < Text.Length; i++)
        {
            char c = Text[i];
            
            // Linebreak
            if (c == '\n')
            {
                pos = new Vector2(originalPos.X, pos.Y + Font.LineHeight);
                continue;
            }

            Sprite sprite = Font.GetCharacterSprite(
                text: Text,
                charIndex: i,
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