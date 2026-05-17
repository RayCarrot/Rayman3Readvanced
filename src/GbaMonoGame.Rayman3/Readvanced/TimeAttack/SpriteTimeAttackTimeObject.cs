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
        return Engine.Assets.FrameContentManager.Load<Texture2D>(timeChar switch
        {
            '0' => Assets.TimeAttack.TimeDigit_0,
            '1' => Assets.TimeAttack.TimeDigit_1,
            '2' => Assets.TimeAttack.TimeDigit_2,
            '3' => Assets.TimeAttack.TimeDigit_3,
            '4' => Assets.TimeAttack.TimeDigit_4,
            '5' => Assets.TimeAttack.TimeDigit_5,
            '6' => Assets.TimeAttack.TimeDigit_6,
            '7' => Assets.TimeAttack.TimeDigit_7,
            '8' => Assets.TimeAttack.TimeDigit_8,
            '9' => Assets.TimeAttack.TimeDigit_9,
            ':' => Assets.TimeAttack.TimeDigit_Colon,
            '.' => Assets.TimeAttack.TimeDigit_Dot,
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