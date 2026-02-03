using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

// Original name: BombePiquante
[GenerateFsmFields]
public sealed partial class SpikyFlyingBomb : MovableActor
{
    public SpikyFlyingBomb(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        SoundDelay = 0;
        Destroyed = false;

        Action firstActionId = (Action)actorResource.FirstActionId;

        // Unused
        if (firstActionId == Action.WaitToAttack)
        {
            State.SetTo(_Fsm_Wait);
        }
        else if (firstActionId == Action.Stationary)
        {
            State.SetTo(_Fsm_Stationary);
        }
        else
        {
            CurrentDirectionalType = firstActionId switch
            {
                Action.Move_Left => PhysicalTypeValue.Enemy_Left,
                Action.Move_Right => PhysicalTypeValue.Enemy_Right,
                Action.Move_Up => PhysicalTypeValue.Enemy_Up,
                Action.Move_Down => PhysicalTypeValue.Enemy_Down,
                _ => throw new Exception("Invalid initial action for the spiky flying bomb")
            };
            State.SetTo(_Fsm_Move);
        }
    }

    public PhysicalTypeValue CurrentDirectionalType { get; set; }
    public byte SoundDelay { get; set; }
    public bool Destroyed { get; set; }

    private void ManageSound()
    {
        if (SoundDelay != 0)
        {
            SoundDelay--;
        }
        else if (AnimatedObject.IsFramed && (GameInfo.ActorSoundFlags & ActorSoundFlags.FlyingBomb) == 0)
        {
            if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__BombFly_Mix03))
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BombFly_Mix03);

            SoundDelay = 60;
        }

        if (AnimatedObject.IsFramed)
            GameInfo.ActorSoundFlags |= ActorSoundFlags.FlyingBomb;
    }

    public override void Step()
    {
        base.Step();
        GameInfo.ActorSoundFlags &= ~ActorSoundFlags.FlyingBomb;
    }
}