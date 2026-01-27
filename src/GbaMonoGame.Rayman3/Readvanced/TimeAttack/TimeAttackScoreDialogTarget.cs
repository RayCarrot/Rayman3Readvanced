using System;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackScoreDialogTarget
{
    public TimeAttackScoreDialogTarget(TimeAttackTime time, RenderContext renderContext, Vector2 position)
    {
        Time = time;
        TimeText = new SpriteTimeAttackTimeObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(0, 20),
            RenderContext = renderContext,
            Time = time,
        };
        BlankIcon = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = position,
            RenderContext = renderContext,
            Texture = time.LoadBigIcon(false)
        };
        FilledInIcon = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = position,
            RenderContext = renderContext,
            Texture = time.LoadBigIcon(true)
        };

        FilledInIconScale = 0;
    }

    public TimeAttackTime Time { get; }
    public SpriteTimeAttackTimeObject TimeText { get; set; }
    public SpriteTextureObject BlankIcon { get; }
    public SpriteTextureObject FilledInIcon { get; }

    public float FilledInIconScale { get; set; }

    public bool IsTransitioningIn { get; set; }
    public float ScaleTimer { get; set; }

    public void TransitionIn()
    {
        IsTransitioningIn = true;
        ScaleTimer = 0;
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        if (IsTransitioningIn)
        {
            // This code is re-implemented from the original Rayman 3 console game
            // code, which is why the values are a bit weird here
            ScaleTimer += 1 / 4f;
            float scaleValue = MathF.Cos(0.4f * ScaleTimer) * 32;

            FilledInIconScale = scaleValue / 6f;
            if (FilledInIconScale <= 1)
            {
                FilledInIconScale = 1;
                IsTransitioningIn = false;

                // 60/40 chance for either sound to play. This could be done through
                // a random resource, but then it wouldn't work for the N-Gage version.
                if (Random.GetNumber(100) < 60)
                    SoundEventsManager.ProcessEvent(ReadvancedSoundEvent.Play__PadStamp01_Mix01);
                else
                    SoundEventsManager.ProcessEvent(ReadvancedSoundEvent.Play__PadStamp02_Mix01);
            }
        }

        TimeText.ScreenPos = BlankIcon.ScreenPos + new Vector2(BlankIcon.Texture.Width / 2f, 38);
        TimeText.ScreenPos -= new Vector2(TimeText.GetWidth() / 2f, 0);
        animationPlayer.Play(TimeText);

        // Draw blank icon if the filled in scale is not 1
        if (FilledInIconScale != 1)
        {
            animationPlayer.Play(BlankIcon);
        }

        // Draw filled in icon if the scale is not 0
        if (FilledInIconScale != 0)
        {
            FilledInIcon.AffineMatrix = new AffineMatrix(0, new Vector2(FilledInIconScale));
            animationPlayer.PlayFront(FilledInIcon);
        }
    }
}