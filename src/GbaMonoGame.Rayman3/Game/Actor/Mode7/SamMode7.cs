using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class SamMode7 : Mode7Actor
{
    public SamMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        ShouldDraw = false;
        AnimatedObject.BgPriority = 0;
        RenderHeight = 16;
        Direction = Angle256.Quarter * 3; // 3/4
        field_0x70 = 1;
        WaterSplashObj = null;
        TetherItems = null;

        State.SetTo(Fsm_Init);
    }

    // TODO: Name and implement
    public byte field_0x70 { get; set; }
    public object TetherItems { get; set; }

    public byte Timer { get; set; }
    public Angle256 TargetDirection { get; set; }
    public WaterSplashMode7 WaterSplashObj { get; set; }
    public bool ShouldDraw { get; set; }

    private void SetMode7DirectionalAction()
    {
        SetMode7DirectionalAction(0, 5);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Main_Damaged2:
                field_0x70 = 0;
                return true;

            case Message.ReloadAnimation:
                // Don't need to do anything. The original game reloads the tether animations here.
                return false;

            default:
                return false;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        // TODO: Create tether items
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (!ShouldDraw)
            return;

        // TODO: Implement drawing tether items
        base.Draw(animationPlayer, forceDraw);
    }
}