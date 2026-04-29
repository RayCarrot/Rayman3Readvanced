using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// Custom object to render time
public class SpriteTimeAttackTimeObject : AObject
{
    private TimeAttackTime _time;
    private string _timeString;

    public TimeAttackTime Time
    {
        get => _time;
        set
        {
            _time = value;
            _timeString = value.ToTimeString();
        }
    }

    public AlphaCoefficient Alpha { get; set; }

    private Texture2D GetTexture(char timeChar)
    {
        return Engine.FrameContentManager.Load<Texture2D>(timeChar switch
        {
            '0' => Assets.TimeDigit_0_Texture,
            '1' => Assets.TimeDigit_1_Texture,
            '2' => Assets.TimeDigit_2_Texture,
            '3' => Assets.TimeDigit_3_Texture,
            '4' => Assets.TimeDigit_4_Texture,
            '5' => Assets.TimeDigit_5_Texture,
            '6' => Assets.TimeDigit_6_Texture,
            '7' => Assets.TimeDigit_7_Texture,
            '8' => Assets.TimeDigit_8_Texture,
            '9' => Assets.TimeDigit_9_Texture,
            ':' => Assets.TimeDigit_ColonTexture,
            '.' => Assets.TimeDigit_DotTexture,
            _ => throw new Exception("Invalid time char"),
        });
    }

    public float GetWidth()
    {
        float width = 0;

        foreach (char timeChar in _timeString)
        {
            Texture2D texture = GetTexture(timeChar);
            width += texture.Width + 1;
        }

        width -= 1;

        return width;
    }

    public float GetHeight()
    {
        return 12;
    }

    public override void Execute(Action<short> soundEventCallback)
    {
        Vector2 pos = GetAnchoredPosition();

        foreach (char timeChar in _timeString)
        {
            Texture2D texture = GetTexture(timeChar);

            Sprite sprite = Gfx.GetNewSprite();
            sprite.Texture = texture;
            sprite.Position = pos;
            sprite.Center = true;
            sprite.Priority = BgPriority;
            sprite.RenderOptions = RenderOptions;
            sprite.Alpha = Alpha;
            Gfx.AddSprite(sprite, SpriteType);

            pos += new Vector2(texture.Width + 1, 0);
        }
    }
}