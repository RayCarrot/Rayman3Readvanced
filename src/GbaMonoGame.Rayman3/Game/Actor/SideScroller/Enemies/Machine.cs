using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Machine : MovableActor
{
    public Machine(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Resource = actorResource;

        if ((Action)actorResource.FirstActionId == Action.CannonIdle1)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachMotr_Mix01, this);
            BossHealth = 4;
            AnimatedObject.ObjPriority = 62;
        }
        else
        {
            BossHealth = -1;
            AnimatedObject.ObjPriority = 63;
        }

        CogDestroyed = false;
        MurfySpawned = false;
        TextBox = null;
        State.SetTo(Fsm_Init);
    }

    // For some reason this array has 10 entries, but only 8 are ever used
    private static readonly Vector2[] _explosionOffsets =
    [
        new(-5, 5),
        new(7, 12),
        new(-20, 50),
        new(40, 22),
        new(-55, 43),
        new(30, 60),
        new(-10, 18),
        new(5, 55),
        new(-34, 20),
        new(60, 32)
    ];

    public ActorResource Resource { get; }
    public TextBoxDialog TextBox { get; set; }
    public Machine Parent { get; set; }
    public int BossHealth { get; set; }
    public byte Rotation { get; set; }
    public bool CogDestroyed { get; set; }
    public ushort Timer { get; set; }
    public bool MurfySpawned { get; set; } // Unused

    private void InitTextBox()
    {
        MurfySpawned = false;
        TextBox = Scene.GetRequiredDialog<TextBoxDialog>();
        TextBox.MoveInOurOut(true);
        Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginCutscene);
    }

    private void UnInitTextBox()
    {
        Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
        TextBox = null;
    }

    private void ManageTextBox()
    {
        if (!TextBox.IsFinished)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.A))
                TextBox.MoveToNextText();
        }
        else
        {
            TextBox.MoveInOurOut(false);
            Timer = 0;
            GameInfo.CanShowMurfyHelp = false;
        }
    }

    private void SpawnHelicopterBomb()
    {
        FlyingBomb helicopterBomb = Scene.CreateProjectile<FlyingBomb>(ActorType.RotatedHelicopterBomb);
        if (helicopterBomb != null)
        {
            helicopterBomb.Position = Position + new Vector2(-60, 10);
            helicopterBomb.CurrentDirectionalType = null;
        }

        Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
        if (explosion != null)
        {
            explosion.AnimatedObject.CurrentAnimation = 1;
            explosion.Position = Position + new Vector2(-45, 0);
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_Hurt:
                if (State == Fsm_CogWheelSpinning)
                {
                    CogDestroyed = true;
                    Parent.ProcessMessage(this, Message.Actor_Explode);
                }
                return false;

            case Message.Murfy_Spawn: // Unused
                MurfySpawned = true;
                return false;

            case Message.Actor_Explode:
                BossHealth--;
                ((FrameSideScroller)Frame.Current).UserInfo.BossHit();
                return false;

            default:
                return false;
        }
    }
}