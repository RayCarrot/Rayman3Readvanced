using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class RaymanMode7 : Mode7Actor
{
    public RaymanMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        SamActorId = actorResource.Links[0]!.Value;
        field_0x7b = 0;
        
        State.SetTo(Fsm_Default);

        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 64;
        
        field_0x7a = Direction;
        SlowDown = false;
        PrevHitPoints = HitPoints;
        ProcessJoypad = false;
        
        GameInfo.IsInWorldMap = false;
    }

    // TODO: Name
    public Angle256 field_0x7a { get; set; }
    public byte field_0x7b { get; set; }

    public float ZPosSpeed { get; set; }
    public float ZPosDeacceleration { get; set; }

    public int SamActorId { get; }
    public bool ProcessJoypad { get; set; }
    public float MoveSpeed { get; set; }
    public bool SlowDown { get; set; }

    public int PrevHitPoints { get; set; }
    public byte InvulnerabilityTimer { get; set; }

    public bool Debug_NoClip { get; set; } // Custom no-clip mode

    private void ToggleNoClip()
    {
        if (InputManager.IsButtonJustPressed(Input.Debug_ToggleNoClip))
        {
            Debug_NoClip = !Debug_NoClip;

            if (Debug_NoClip)
                MechModel.Speed = Vector2.Zero;
            else
                State.MoveTo(Fsm_Default);
        }
    }

    private void DoNoClipBehavior()
    {
        Vector2 direction = Direction.ToDirectionalVector() * new Vector2(1, -1);
        Vector2 sideDirection = (Direction + Angle256.Quarter).ToDirectionalVector() * new Vector2(1, -1);

        int speed = JoyPad.IsButtonPressed(GbaInput.A) ? 4 : 2;

        if (JoyPad.IsButtonPressed(GbaInput.Up))
            Position += direction * speed;
        if (JoyPad.IsButtonPressed(GbaInput.Down))
            Position -= direction * speed;

        if (JoyPad.IsButtonPressed(GbaInput.Right))
            Position -= sideDirection * speed;
        if (JoyPad.IsButtonPressed(GbaInput.Left))
            Position += sideDirection * speed;

        if (JoyPad.IsButtonPressed(GbaInput.R))
            Direction--;
        if (JoyPad.IsButtonPressed(GbaInput.L))
            Direction++;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.MainMode7_CollectedYellowLum:
                ((FrameWaterSkiMode7)Frame.Current).UserInfo.LumsBar.AddLums(1);
                return true;

            case Message.Main_LevelEnd:
                ProcessJoypad = false;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                LevelMusicManager.HasOverridenLevelMusic = false;
                return true;

            case Message.MainMode7_LevelStart:
                ProcessJoypad = true;
                return true;

            case (Message)1085: // TODO: Name. The message comes from a captor, but does nothing. Check prototypes?
                // Do nothing
                return true;

            default:
                return false;
        }
    }

    public override void DoBehavior()
    {
        if (Debug_NoClip)
            DoNoClipBehavior();
        else
            base.DoBehavior();
    }

    public override void Step()
    {
        base.Step();
        ToggleNoClip();
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // TODO: Implement
        base.Draw(animationPlayer, forceDraw);
    }
}