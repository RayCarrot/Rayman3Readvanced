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
    public bool IsAlphaBlendEnabled { get; set; }

    public float Alpha { get; set; } = 1;
    public float GbaAlpha
    {
        get => Alpha * 16;
        set => Alpha = value / 16;
    }

    public int GetStringWidth() => FontManager.GetStringWidth(FontSize, TextBytes);

    public override void Execute(Action<short> soundEventCallback)
    {
        if (TextBytes == null)
            return;

        Vector2 pos = GetAnchoredPosition();

        foreach (byte c in TextBytes)
        {
            // TODO: Option to always draw with the highest resolution font (but scale to fit original size)
            Gfx.AddSprite(FontManager.GetCharacterSprite(
                c: c, 
                fontSize: FontSize, 
                position: ref pos, 
                priority: BgPriority, 
                affineMatrix: AffineMatrix, 
                alpha: IsAlphaBlendEnabled ? Alpha : null, 
                color: Color, 
                renderContext: RenderContext));
        }
    }
}