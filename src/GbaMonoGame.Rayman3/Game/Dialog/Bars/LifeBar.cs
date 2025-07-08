using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework.Graphics;

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

    public SpriteTextureObject InfiniteSymbol { get; set; }

    private void LoadInfiniteSymbolIfNeeded()
    {
        if (Engine.Config.Difficulty.InfiniteLives && InfiniteSymbol == null)
        {
            InfiniteSymbol = new SpriteTextureObject
            {
                Texture = Engine.FixContentManager.Load<Texture2D>(Assets.Hud_Infinity),
                ScreenPos = new Vector2(40, 6),
                BgPriority = 0,
                ObjPriority = 0,
                RenderContext = Scene.HudRenderContext,
            };
        }
    }

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

        LoadInfiniteSymbolIfNeeded();
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
        // Check if close to dead
        else if (hp == 1 && (GameTime.ElapsedFrames & 0x3f) == 0x3f && !Engine.Config.Difficulty.OneHitPoint && !Engine.Config.Sound.DisableLowHealthSound)
        {
            // NOTE: There's a bug where if you pause on the same frame as this sound should be playing then it
            //       will keep playing every single frame! Optionally fix by checking so the time isn't paused.
            if (!(GameTime.IsPaused && Engine.Config.Tweaks.FixBugs))
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
            // NOTE: The game only checks if less than 16, but this includes animation 10 which is the 0 hp one and
            //       has no second variant! This originally makes it display the wrong animation.
            if (!Engine.Config.Tweaks.FixBugs)
            {
                if (HitPoints.CurrentAnimation < 16)
                    HitPoints.CurrentAnimation += 5;
            }
            else
            {
                if (HitPoints.CurrentAnimation is > 10 and < 16)
                    HitPoints.CurrentAnimation += 5;
            }

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
            if (Engine.Config.Difficulty.OneHitPoint)
            {
                HitPoints.DeactivateChannel(2);
                HitPoints.DeactivateChannel(3);
                HitPoints.DeactivateChannel(4);
                HitPoints.DeactivateChannel(5);

                if (HitPoints.CurrentAnimation != 10)
                    HitPoints.CurrentAnimation = 20;
            }
            else
            {
                HitPoints.ActivateAllChannels();
            }

            HitPoints.ScreenPos = HitPoints.ScreenPos with { Y = 0 - OffsetY };

            if (!Engine.Config.Difficulty.InfiniteLives)
            {
                LifeDigit1.ScreenPos = LifeDigit1.ScreenPos with { Y = 20 - OffsetY };
                LifeDigit2.ScreenPos = LifeDigit2.ScreenPos with { Y = 20 - OffsetY };

                animationPlayer.PlayFront(HitPoints);
                animationPlayer.PlayFront(LifeDigit1);
                animationPlayer.PlayFront(LifeDigit2);
            }
            else
            {
                LoadInfiniteSymbolIfNeeded();

                InfiniteSymbol.ScreenPos = InfiniteSymbol.ScreenPos with { Y = 6 - OffsetY };

                animationPlayer.PlayFront(HitPoints);
                animationPlayer.PlayFront(InfiniteSymbol);
            }
        }
    }
}