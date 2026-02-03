using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class BluePirate : PirateBaseActor
{
    public BluePirate(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        SpawnsRedLum = ActionId is Action.Init_HasRedLum_Right or Action.Init_HasRedLum_Left;
        ReInit();

        _debugChainAttackBoxAObject = new DebugBoxAObject()
        {
            Color = DebugBoxColor.AttackBox,
            RenderContext = Scene.RenderContext
        };
    }

    private readonly DebugBoxAObject _debugChainAttackBoxAObject;

    private Box _lastChainAttackBox;

    public bool QueueFallSound { get; set; } // Custom to prevent fall sounds from playing on level load when playing with all objects loaded

    private Box GetChainAttackBox(float offsetX)
    {
        Box box = new(offsetX, -16, offsetX + 16, 0);

        if (AnimatedObject.FlipX)
            box = Box.FlipX(box);

        _lastChainAttackBox = box;
        return Box.Offset(box, Position);
    }

    public override void DoBehavior()
    {
        _lastChainAttackBox = Box.Empty;
        base.DoBehavior();
    }

    protected override void ReInit()
    {
        State.SetTo(_Fsm_Fall);
        ChangeAction();
    }

    public override void Step()
    {
        base.Step();

        // Custom to prevent fall sounds from playing on level load when playing with all objects loaded
        if (QueueFallSound && AnimatedObject.IsFramed)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraJump_BigFoot1_Mix02);
            QueueFallSound = false;
        }
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        if (Scene.Camera.IsDebugBoxFramed(_debugChainAttackBoxAObject, Position + _lastChainAttackBox.Position))
        {
            _debugChainAttackBoxAObject.Size = _lastChainAttackBox.Size;
            animationPlayer.PlayFront(_debugChainAttackBoxAObject);
        }
    }
}