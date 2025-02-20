using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: Mumu
public sealed partial class Caterpillar : MovableActor
{
    public Caterpillar(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource, new AObjectChain(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic))
    {
        Direction = MoveDirection.Left;
        CheckAgainstMapCollision = false;

        AnimatedObject.Init(6, Position, 2, false);

        SineWaveValue = 0;
        LastPosition = Position;

        switch ((Action)actorResource.FirstActionId)
        {
            case Action.Move_Right:
                Direction = MoveDirection.Right;
                MechModel.Speed = new Vector2(1, 0);
                break;

            case Action.Move_Left:
                Direction = MoveDirection.Left;
                MechModel.Speed = new Vector2(-1, 0);
                break;

            case Action.Move_Up:
                Direction = MoveDirection.Up;
                MechModel.Speed = new Vector2(0, -1);
                break;
            
            case Action.Move_Down:
                Direction = MoveDirection.Down;
                MechModel.Speed = new Vector2(0, 1);
                break;

            default:
                throw new Exception("Invalid initial action set for the caterpillar actor");
        }

        State.SetTo(Fsm_Move);
    }

    public new AObjectChain AnimatedObject => (AObjectChain)base.AnimatedObject;

    public Vector2 LastPosition { get; set; }
    public Vector2 ReturnSpeed { get; set; }
    public MoveDirection Direction { get; set; }
    public byte SineWaveValue { get; set; }
    public float TargetPosition { get; set; }
    public ushort Timer { get; set; }

    private void CheckMoveBounds()
    {
        Vector2 pos;

        if (Direction is MoveDirection.Up or MoveDirection.Down)
        {
            LastPosition = LastPosition with { Y = Position.Y };
            pos = new Vector2(LastPosition.X, Position.Y);
        }
        else if (Direction is MoveDirection.Left or MoveDirection.Right)
        {
            LastPosition = LastPosition with { X = Position.X };
            pos = new Vector2(Position.X, LastPosition.Y);
        }
        else
        {
            throw new Exception("Invalid direction");
        }

        PhysicalType type = Scene.GetPhysicalType(pos);

        switch (type.Value)
        {
            case PhysicalTypeValue.Enemy_Left:
                MechModel.Speed = new Vector2(-1, 0);

                if (Direction is MoveDirection.Up or MoveDirection.Down)
                {
                    LastPosition = LastPosition with { Y = Position.Y };
                    SineWaveValue = 0;
                }

                Direction = MoveDirection.Left;
                ActionId = Action.Move_Left;
                break;

            case PhysicalTypeValue.Enemy_Right:
                MechModel.Speed = new Vector2(1, 0);

                if (Direction is MoveDirection.Up or MoveDirection.Down)
                {
                    LastPosition = LastPosition with { Y = Position.Y };
                    SineWaveValue = 0;
                }

                Direction = MoveDirection.Right;
                ActionId = Action.Move_Right;
                break;

            case PhysicalTypeValue.Enemy_Up:
                MechModel.Speed = new Vector2(0, -1);

                if (Direction is MoveDirection.Left or MoveDirection.Right)
                {
                    LastPosition = LastPosition with { X = Position.X };
                    SineWaveValue = 0;
                }

                Direction = MoveDirection.Up;
                ActionId = Action.Move_Up;
                break;

            case PhysicalTypeValue.Enemy_Down:
                MechModel.Speed = new Vector2(0, 1);

                if (Direction is MoveDirection.Left or MoveDirection.Right)
                {
                    LastPosition = LastPosition with { X = Position.X };
                    SineWaveValue = 0;
                }

                Direction = MoveDirection.Down;
                ActionId = Action.Move_Down;
                break;
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.ThrowObjectUp:
                State.MoveTo(Fsm_ThrownUp);
                return false;

            case Message.ThrowObjectForward:
                State.MoveTo(Fsm_ThrownForward);
                return false;

            case Message.DropObject:
                State.MoveTo(Fsm_Projectile);
                return false;

            // Unused
            case Message.HitActorOfSameType:
                State.MoveTo(Fsm_Dying);
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        AnimatedObject.Draw(this, animationPlayer, forceDraw);
    }

    public enum MoveDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }
}