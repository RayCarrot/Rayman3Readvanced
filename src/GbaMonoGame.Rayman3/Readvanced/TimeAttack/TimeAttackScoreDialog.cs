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
            Select((x, i) => new TimeTarget(x, Scene.HudRenderContext, new Vector2(96 + i * 80, 140))).
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
            Blank = new SpriteTextureObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = position,
                RenderContext = renderContext,
                Texture = time.LoadBigIcon(false)
            };
            FilledIn = new SpriteTextureObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = position,
                RenderContext = renderContext,
                Texture = time.LoadBigIcon(true)
            };

            FilledInStarScale = 0;
        }

        public TimeAttackTime Time { get; }
        public SpriteTextureObject Blank { get; }
        public SpriteTextureObject FilledIn { get; }

        public float FilledInStarScale { get; set; }
        
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

                FilledInStarScale = scaleValue / 6f;
                if (FilledInStarScale <= 1)
                {
                    FilledInStarScale = 1;
                    IsTransitioningIn = false;

                    // 60/40 chance for either sound to play. This could be done through
                    // a random resource, but then it wouldn't work for the N-Gage version.
                    if (Random.GetNumber(100) < 60)
                        SoundEventsManager.ProcessEvent(ReadvancedSoundEvent.Play__PadStamp01_Mix01);
                    else
                        SoundEventsManager.ProcessEvent(ReadvancedSoundEvent.Play__PadStamp02_Mix01);
                }
            }

            // Draw blank star if the filled in scale is not 1
            if (FilledInStarScale != 1)
            {
                animationPlayer.Play(Blank);
            }

            // Draw filled in star if the scale is not 0
            if (FilledInStarScale != 0)
            {
                FilledIn.AffineMatrix = new AffineMatrix(0, new Vector2(FilledInStarScale));
                animationPlayer.Play(FilledIn);
            }
        }
    }
}