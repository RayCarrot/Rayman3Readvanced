using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Lums : BaseActor
{
    public Lums(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        // NOTE: In the game it creates a special version of the AnimatedObject for this class called AObjectLum.
        //       That allows a palette to be defined, and doesn't handle things like sound events, boxes etc. We
        //       can however keep using the default AnimatedObject class here.
        //       Also, for multiplayer it doesn't change the palette index, but instead replaced palette 1 with
        //       palette 0, as to have to avoid two palettes when only one is used (since only blue lums appear
        //       in the multiplayer level).

        LumId = 0;
        ActionId = (Action)actorResource.FirstActionId;

        switch (ActionId)
        {
            case Action.YellowLum:
            case Action.RedLum:
            case Action.GreenLum:
                AnimatedObject.CurrentAnimation = (byte)ActionId * 3;

                if (ActionId == Action.YellowLum && !IsProjectile && !RSMultiplayer.IsActive)
                {
                    LumId = GameInfo.GetLumsId();

                    if (GameInfo.IsLumDead(LumId, GameInfo.MapId))
                        ProcessMessage(this, Message.Destroy);
                }
                break;

            case Action.BlueLum:
                AnimatedObject.BasePaletteIndex = 1;
                AnimatedObject.CurrentAnimation = 0;
                break;

            case Action.WhiteLum:
                AnimatedObject.BasePaletteIndex = 1;

                if (GameInfo.HasCollectedWhiteLum && !RSMultiplayer.IsActive)
                    ProcessMessage(this, Message.Destroy);

                AnimatedObject.CurrentAnimation = 3;
                break;

            case Action.BigYellowLum:
                AnimatedObject.CurrentAnimation = 10;
                break;

            case Action.BigBlueLum:
                AnimatedObject.BasePaletteIndex = 1;
                AnimatedObject.CurrentAnimation = 10;
                break;

            default:
                throw new Exception($"Unknown lum state {ActionId}");
        }

        if (!RSMultiplayer.IsActive)
        {
            State.SetTo(Fsm_Idle);

            if (ActionId == Action.GreenLum)
            {
                LumId = GameInfo.GetGreenLumsId();

                if (GameInfo.IsGreenLumDead(LumId))
                    ProcessMessage(this, Message.Destroy);
            }
        }
        else
        {
            State.SetTo(Fsm_MultiplayerIdle);
            Timer = 0xFF;
            LumId = instanceId;
            MultiplayerInfo.TagInfo.SaveLumPosition(instanceId, actorResource);
        }
    }

    public int LumId { get; }
    public byte Timer { get; set; }

    private Box GetCollisionBox()
    {
        Box viewBox = GetViewBox();
        viewBox.MinX += 16;
        viewBox.MinY += 16;
        viewBox.MaxX -= 16;
        viewBox.MaxY -= 16;
        return viewBox;
    }

    private bool CheckCollision()
    {
        return Scene.MainActor.GetDetectionBox().Intersects(GetCollisionBox());
    }

    private bool CheckCollisionAndAttract(Box playerBox)
    {
        bool collided = playerBox.Intersects(GetCollisionBox());

        // Move the lum towards the main actor
        if (!collided)
        {
            Vector2 detectionCenter = playerBox.Center;

            if (Position.X < detectionCenter.X)
                Position += new Vector2(3.5f, 0);
            else
                Position -= new Vector2(3.5f, 0);

            if (Position.Y < detectionCenter.Y)
                Position += new Vector2(0, 3.5f);
            else
                Position -= new Vector2(0, 3.5f);
        }

        return collided;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Intercept messages
        switch (message)
        {
            // Don't resurrect if collected (used when triggered from captors)
            case Message.Resurrect:
                if (ActionId == Action.YellowLum && GameInfo.IsLumDead(LumId, GameInfo.MapId))
                    return false;
                break;
        }

        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Lum_ToggleVisibility:
                if (State.IsSet)
                    State.MoveTo(null);
                else
                    State.MoveTo(Fsm_Idle);
                return false;

            case Message.Actor_ReloadAnimation:
                // Don't need to do anything. The original game sets the palette index again, but we're using local indexes, so it never changes.
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (State != Fsm_MultiplayerDelay && State != Fsm_Delay && State.IsSet)
        {
            if (Scene.Camera.IsActorFramed(this))
            {
                AnimatedObject.IsFramed = true;
                animationPlayer.Play(AnimatedObject);
            }
            else
            {
                AnimatedObject.IsFramed = false;

                if (RSMultiplayer.IsActive)
                    AnimatedObject.ComputeNextFrame();
            }
        }
    }
}