using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class LifeBar : Bar
{
    public LifeBar(Scene2D scene) : base(scene) { }

    public int WaitTimer { get; set; }
    public int OffsetY { get; set; }
    public int PreviousLivesCount { get; set; }
    public int PreviousHitPoints { get; set; }
    public bool HitPointsChanged { get; set; }

    public AnimatedObject HitPoints { get; set; }
    public AnimatedObject LifeDigit1 { get; set; }
    public AnimatedObject LifeDigit2 { get; set; }

    public void UpdateLife()
    {
        if (DrawStep != BarDrawStep.Bounce && Mode != BarMode.StayHidden)
            DrawStep = BarDrawStep.MoveIn;
    }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.HudAnimations);
        
        HitPoints = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 15,
            ScreenPos = new Vector2(-4, 0),
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };
        
        LifeDigit1 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(49, 20),
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };
        
        LifeDigit2 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(61, 20),
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };
    }

    public override void Set()
    {
        LifeDigit1.CurrentAnimation = GameInfo.PersistentInfo.Lives / 10;
        LifeDigit2.CurrentAnimation = GameInfo.PersistentInfo.Lives % 10;
        HitPoints.CurrentAnimation = 15 + Scene.MainActor.HitPoints;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Mode is BarMode.StayHidden or BarMode.Disabled)
            return;

        int hp = Scene.MainActor.HitPoints;

        // Check if lives count has changed
        if (PreviousLivesCount != GameInfo.PersistentInfo.Lives)
        {
            PreviousLivesCount = GameInfo.PersistentInfo.Lives;

            LifeDigit1.CurrentAnimation = GameInfo.PersistentInfo.Lives / 10;
            LifeDigit2.CurrentAnimation = GameInfo.PersistentInfo.Lives % 10;

            DrawStep = BarDrawStep.MoveIn;
            WaitTimer = 0;
        }

        // Check if dead
        if (hp == 0 && DrawStep == BarDrawStep.Wait)
        {
            HitPoints.CurrentAnimation = 10;
         
            DrawStep = BarDrawStep.Wait;
            WaitTimer = 0;
        }
        // TODO: Have option not to play this sound because it's annoying
        // Check if close to dead
        else if (hp == 1 && (GameTime.ElapsedFrames & 0x3f) == 0x3f)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MinHP);
        }

        // Check if hp has changed
        if (PreviousHitPoints != hp)
        {
            PreviousHitPoints = hp;

            HitPoints.CurrentAnimation = 10 + Scene.MainActor.HitPoints;

            HitPointsChanged = true;
            DrawStep = BarDrawStep.MoveIn;
            WaitTimer = 0;
        }
        else if (HitPointsChanged && HitPoints.EndOfAnimation)
        {
            if (HitPoints.CurrentAnimation < 16)
                HitPoints.CurrentAnimation += 5;

            HitPointsChanged = false;
        }

        switch (DrawStep)
        {
            case BarDrawStep.Hide:
                OffsetY = 36;
                break;

            case BarDrawStep.MoveIn:
                if (OffsetY != 0)
                {
                    OffsetY -= 2;
                }
                else
                {
                    DrawStep = BarDrawStep.Wait;
                    WaitTimer = 0;
                }
                break;

            case BarDrawStep.MoveOut:
                if (OffsetY < 36)
                {
                    OffsetY += 2;
                }
                else
                {
                    OffsetY = 36;
                    DrawStep = BarDrawStep.Hide;
                }
                break;

            case BarDrawStep.Wait:
                if (Mode != BarMode.StayVisible)
                {
                    if (WaitTimer >= 360)
                    {
                        OffsetY = 0;
                        DrawStep = BarDrawStep.MoveOut;
                    }
                    else
                    {
                        WaitTimer++;
                    }
                }
                break;
        }

        if (DrawStep != BarDrawStep.Hide)
        {
            HitPoints.ScreenPos = HitPoints.ScreenPos with { Y = 0 - OffsetY };
            LifeDigit1.ScreenPos = LifeDigit1.ScreenPos with { Y = 20 - OffsetY };
            LifeDigit2.ScreenPos = LifeDigit2.ScreenPos with { Y = 20 - OffsetY };

            animationPlayer.PlayFront(HitPoints);
            animationPlayer.PlayFront(LifeDigit1);
            animationPlayer.PlayFront(LifeDigit2);
        }
    }
}