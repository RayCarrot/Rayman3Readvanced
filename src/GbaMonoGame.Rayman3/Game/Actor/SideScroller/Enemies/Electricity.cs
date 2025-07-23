using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Electricity : InteractableActor
{
    public Electricity(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialActionId = (Action)actorResource.FirstActionId;

        float left = Position.X;
        if (InitialActionId == Action.DoubleActivated_Left)
        {
            left += 58;

            // The hitbox is misaligned in the original game as it doesn't account for the width here
            if (Engine.ActiveConfig.Tweaks.FixBugs)
                left -= 12;
        }
        else if (InitialActionId == Action.DoubleActivated_Right)
        {
            left -= 58;
        }

        AdditionalAttackBox = new Box(
            left: left, 
            top: Position.Y - 12, 
            right: left + 12, 
            bottom: Position.Y + 20);

        State.SetTo(Fsm_Activated);

        _debugAdditionalAttackBoxAObject = new DebugBoxAObject()
        {
            Color = DebugBoxColor.AttackBox,
            RenderContext = Scene.RenderContext
        };
    }

    private readonly DebugBoxAObject _debugAdditionalAttackBoxAObject;

    public Action InitialActionId { get; }
    public Box AdditionalAttackBox { get; }

    public override void Step()
    {
        base.Step();

        if ((GameInfo.ActorSoundFlags & ActorSoundFlags.Electricity) == 0)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Electric_Mix02);
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        GameInfo.ActorSoundFlags &= ~ActorSoundFlags.Electricity;

        if (Scene.Camera.IsActorFramed(this) || forceDraw)
        {
            AnimatedObject.IsFramed = true;
            AnimatedObject.FrameChannelSprite(Position, new Box(Scene.Playfield.Camera.Position, AnimatedObject.RenderContext.Resolution));
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        if (InitialActionId is Action.DoubleActivated_Left or Action.DoubleActivated_Right &&
            State == Fsm_Activated &&
            Scene.Camera.IsDebugBoxFramed(_debugAdditionalAttackBoxAObject, AdditionalAttackBox.Position))
        {
            _debugAdditionalAttackBoxAObject.Size = AdditionalAttackBox.Size;
            animationPlayer.PlayFront(_debugAdditionalAttackBoxAObject);
        }
    }
}