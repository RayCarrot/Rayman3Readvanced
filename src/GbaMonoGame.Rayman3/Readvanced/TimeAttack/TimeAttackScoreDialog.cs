using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Fully implement this
public class TimeAttackScoreDialog : Dialog
{
    public TimeAttackScoreDialog(Scene2D scene) : base(scene)
    {
        Timer = 0;
    }

    public TimeTarget[] TimeTargets { get; set; }
    public uint TimeTargetTransitionIndex { get; set; }
    public uint Timer { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public override void Load()
    {
        TimeTargets = TimeAttackInfo.TargetTimes.
            Where(x => x.Type != TimeAttackTimeType.Record).
            Select((x, i) => new TimeTarget(x, Scene.HudRenderContext, new Vector2(80 + i * 96, 140))).
            ToArray();
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        Timer++;
        
        if (TimeTargetTransitionIndex != TimeTargets.Length)
        {
            if (Timer == 50)
            {
                TimeTargets[TimeTargetTransitionIndex].TransitionIn();
                TimeTargetTransitionIndex++;
                Timer = 0;
            }
        }
        else
        {
            if (Timer == 50)
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
            }
        }

        foreach (TimeTarget timeTarget in TimeTargets)
            timeTarget.Draw(animationPlayer);
    }

    public class TimeTarget
    {
        public TimeTarget(TimeAttackTime time, RenderContext renderContext, Vector2 position)
        {
            Time = time;
            TimeText = new SpriteFontTextObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = position + new Vector2(0, 20),
                RenderContext = renderContext,
                Text = time.ToTimeString(),
                Font = ReadvancedFonts.MenuYellow,
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
        public SpriteFontTextObject TimeText { get; set; }
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

            TimeText.ScreenPos = BlankIcon.ScreenPos + new Vector2(BlankIcon.Texture.Width / 2f, 50);
            TimeText.ScreenPos -= new Vector2(TimeText.Font.GetWidth(TimeText.Text) / 2f, 0);
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
                animationPlayer.Play(FilledInIcon);
            }
        }
    }
}