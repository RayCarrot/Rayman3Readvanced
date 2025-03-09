using System;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.AnimEngine;

// On GBA setting the position is inlined. X is set using the mask 0xfe00 and Y is set using the mask 0xff00.
public class SpriteTextObject : AObject
{
    private string _text;

    private byte[] TextBytes { get; set; }
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            TextBytes = FontManager.GetTextBytes(value);
        }
    }

    public Color Color { get; set; }
    public FontSize FontSize { get; set; }
    public AffineMatrix? AffineMatrix { get; set; }

    public float Alpha { get; set; } = 1;
    public float GbaAlpha
    {
        get => Alpha * 16;
        set => Alpha = value / 16;
    }

    public int GetStringWidth() => FontManager.GetStringWidth(FontSize, TextBytes);

    public void SetYScaling(float scale)
    {
        AffineMatrix = new AffineMatrix(1, 0, 0, scale);
    }

    public override void Execute(Action<short> soundEventCallback)
    {
        if (TextBytes == null)
            return;

        Vector2 originalPos = GetAnchoredPosition();

        // Vertically center so that the Y scaling works
        float height = FontManager.GetFontHeight(FontSize);
        Vector2 origin = new(0, height / 2f);
        Matrix transformation = FontManager.CreateTextTransformation(originalPos, AffineMatrix?.Scale ?? Vector2.One, origin);

        Vector2 pos = Vector2.Zero;

        foreach (byte c in TextBytes)
        {
            // Custom code to allow line-breaks. This isn't supported in the original game.
            if (c == '\r')
                continue;

            if (c == '\n')
            {
                pos.X = 0;
                pos.Y += height;
                continue;
            }

            // TODO: Option to always draw with the highest resolution font (but scale to fit original size)
            Gfx.AddSprite(FontManager.GetCharacterSprite(
                c: c, 
                fontSize: FontSize, 
                transformation: transformation,
                position: ref pos, 
                priority: BgPriority, 
                affineMatrix: AffineMatrix, 
                alpha: Alpha, 
                color: Color, 
                renderOptions: RenderOptions));
        }
    }
}