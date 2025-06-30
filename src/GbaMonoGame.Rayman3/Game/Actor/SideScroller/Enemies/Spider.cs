using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Spider : MovableActor
{
    public Spider(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        SpawnTimer = 0xFF;
        SoundTimer = 0;

        // NOTE: The game doesn't set this, but rather keeps it uninitialized which means it defaults to 0xCD (the
        //       default allocation value), which is seen as true (non-zero).
        ShouldJump = true;

        // Guard
        if ((Action)actorResource.FirstActionId == Action.Stop_Down)
            State.SetTo(Fsm_GuardSpawn);
        // Chase
        else
            State.SetTo(Fsm_ChaseSpawn);
    }

    public Vector2 InititialPosition { get; set; }
    public Action InitialActionId { get; set; }
    public sbyte Timer { get; set; }
    public byte SpawnTimer { get; set; }
    public byte AnimationTimer { get; set; }
    public byte SoundTimer { get; set; }
    public int ClimbSpeedX { get; set; }
    public int ClimbSpeedY { get; set; }
    public bool IsSpiderFacingLeft { get; set; }
    public bool ShouldJump { get; set; }
    public bool IsNotAttacking { get; set; }

    private void UpdateMusic()
    {
        LevelMusicManager.PlaySpecialMusicIfDetectedWith(this, new Box(-127, -127, 127, 127));
    }

    private void UpdateSound()
    {
        if (SoundTimer != 0)
        {
            SoundTimer--;
            if (SoundTimer == 0)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoPeur1_Mix03);
        }
    }

    private void ManageMainActorCollision()
    {
        if (Scene.IsHitMainActor(this))
        {
            Scene.MainActor.ReceiveDamage(AttackPoints);

            if (SoundTimer == 0)
                SoundTimer = 40;
        }
    }

    private void ManageCollision()
    {
        ShouldJump = false;

        PhysicalType type = Scene.GetPhysicalType(Position);
        
        // Fully stop
        if ((!IsSpiderFacingLeft && type == PhysicalTypeValue.Spider_Right) ||
            (IsSpiderFacingLeft && type == PhysicalTypeValue.Spider_Left))
        {
            ClimbSpeedX = 0;
            ClimbSpeedY = 0;
        }
        // Move up
        else if (type == PhysicalTypeValue.Spider_Up)
        {
            ClimbSpeedY = -7;
        }
        // Move down
        else if (type == PhysicalTypeValue.Spider_Down)
        {
            ClimbSpeedY = 7;
        }
        // Full stop (vertical only)
        else if ((!IsSpiderFacingLeft && type == PhysicalTypeValue.Spider_Left) || 
                 (IsSpiderFacingLeft && type == PhysicalTypeValue.Spider_Right))
        {
            ClimbSpeedY = 0;
        }
        // Jump
        else if (!IsSpiderFacingLeft && type == PhysicalTypeValue.Enemy_Right)
        {
            ShouldJump = true;
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Actor_Start:
                if (State == Fsm_ChaseSpawn)
                {
                    Rayman rayman = (Rayman)Scene.MainActor;
                    if (rayman.State == rayman.Fsm_Climb)
                    {
                        SpawnTimer = 0;
                        
                        Scene.Camera.ProcessMessage(this, Message.Cam_MoveToTarget, InititialPosition - new Vector2(60, 60));
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);

                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__ancients);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Horror_Mix08);
                    }
                }
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        AnimatedObject.FrameChannelSprite(Position, new Box(Scene.Playfield.Camera.Position, AnimatedObject.RenderContext.Resolution));

        // NOTE: The original game doesn't do this, but it produces the same result, and we don't want the spider to be shown before
        //       spawned in. The way this works in the original game is that FrameChannelSprite only runs once (cause of the delay
        //       mode), and since it's off-screen first this makes all channels stay deactivated until the animation updates. The
        //       reason we have to do this extra step to ensure they stay deactivated is because the game might be running in a high
        //       enough resolution where the spider is initially on screen, and because framing affine channel sprites isn't implemented.
        if (State == Fsm_ChaseSpawn && SpawnTimer == 0xFF)
            AnimatedObject.DeactivateAllChannels();

        base.Draw(animationPlayer, forceDraw);
    }
}