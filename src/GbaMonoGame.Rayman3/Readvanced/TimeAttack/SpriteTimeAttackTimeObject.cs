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

    private Texture2D GetTexture(char timeChar)
    {
        return Engine.FrameContentManager.Load<Texture2D>(timeChar switch
        {
            '0' => Assets.TimeDigit_0,
            '1' => Assets.TimeDigit_1,
            '2' => Assets.TimeDigit_2,
            '3' => Assets.TimeDigit_3,
            '4' => Assets.TimeDigit_4,
            '5' => Assets.TimeDigit_5,
            '6' => Assets.TimeDigit_6,
            '7' => Assets.TimeDigit_7,
            '8' => Assets.TimeDigit_8,
            '9' => Assets.TimeDigit_9,
            ':' => Assets.TimeDigit_Colon,
            '.' => Assets.TimeDigit_Dot,
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

            Gfx.AddSprite(new Sprite
            {
                Texture = texture,
                Position = pos,
                Center = true,
                Priority = BgPriority,
                RenderOptions = RenderOptions,
            });

            pos += new Vector2(texture.Width + 1, 0);
        }
    }
}