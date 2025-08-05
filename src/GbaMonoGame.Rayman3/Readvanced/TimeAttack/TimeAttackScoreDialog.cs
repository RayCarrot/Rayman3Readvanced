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
            if (Timer == 60)
            {
                TimeTargets[TimeTargetTransitionIndex].TransitionIn();
                TimeTargetTransitionIndex++;
                Timer = 0;
            }
        }
        else
        {
            if (Timer == 60)
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

            Scale = 0;
        }

        public TimeAttackTime Time { get; }
        public SpriteTextureObject Blank { get; }
        public SpriteTextureObject FilledIn { get; }

        public float Scale { get; set; }

        public void TransitionIn()
        {
            Scale = 4;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumSlvr_Mix02);
        }

        public void Draw(AnimationPlayer animationPlayer)
        {
            if (Scale > 1)
            {
                Scale -= 3 / 25f;

                if (Scale < 1)
                    Scale = 1;
            }

            animationPlayer.Play(Blank);

            FilledIn.AffineMatrix = new AffineMatrix(0, new Vector2(Scale));
            animationPlayer.Play(FilledIn);
        }
    }
}