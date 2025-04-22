using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using ImGuiNET;

namespace GbaMonoGame.Engine2d;

public abstract class ActionActor : BaseActor
{
    protected ActionActor(int instanceId, Scene2D scene, ActorResource actorResource)
        : this(instanceId, scene, actorResource, new AnimatedObject(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic)) { }

    protected ActionActor(int instanceId, Scene2D scene, ActorResource actorResource, AnimatedObject animatedObject) 
        : base(instanceId, scene, actorResource, animatedObject)
    {
        HitPoints = actorResource.Model.HitPoints;
        AttackPoints = actorResource.Model.AttackPoints;
        
        Actions = actorResource.Model.Actions;
        ActionId = actorResource.FirstActionId;
        ChangeAction();

        _detectionBox = new Box(actorResource.Model.DetectionBox);

        _debugDetectionBoxAObject = new DebugBoxAObject()
        {
            Color = DebugBoxColor.DetectionBox,
            RenderContext = Scene.RenderContext,
        };
        _debugActionBoxAObject = new DebugBoxAObject()
        {
            Color = DebugBoxColor.ActionBox,
            RenderContext = Scene.RenderContext,
        };
    }

    private int _actionId;
    private Box _detectionBox;
    private Box _actionBox;
    private readonly DebugBoxAObject _debugDetectionBoxAObject;
    private readonly DebugBoxAObject _debugActionBoxAObject;

    public Action[] Actions { get; }

    public int HitPoints { get; set; }
    public int AttackPoints { get; set; }

    public sealed override int ActionId
    {
        get => _actionId;
        set
        {
            _actionId = value;
            NewAction = true;
        }
    }

    public bool NewAction { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Intercept messages
        switch (message)
        {
            case Message.Resurrect:
            case Message.ResurrectWakeUp:
                HitPoints = ActorModel.HitPoints;
                break;
        }

        return base.ProcessMessageImpl(sender, message, param);
    }

    protected void DrawWithInvulnerability(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawWithInvulnerability(animationPlayer, forceDraw, IsInvulnerable);
    }

    protected void DrawWithInvulnerability(AnimationPlayer animationPlayer, bool forceDraw, bool isInvulnerable)
    {
        CameraActor camera = Scene.Camera;

        bool draw = camera.IsActorFramed(this) || forceDraw;

        // NOTE: Instead of checking ElapsedFrames the N-Gage version checks the amount of drawn frames here (since it renders in 30fps)

        // Conditionally don't draw every second frame during invulnerability
        if (draw)
        {
            if (isInvulnerable &&
                HitPoints != 0 &&
                (GameTime.ElapsedFrames & 1) == 0)
            {
                draw = false;
            }
        }

        if (draw)
        {
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }

    public virtual Box GetDetectionBox() => Box.Offset(_detectionBox, Position);
    public virtual Box SetDetectionBox(Box detectionBox) => _detectionBox = detectionBox;
    public virtual Box GetActionBox() => Box.Offset(_actionBox, Position);

    public void ChangeAction()
    {
        if (!NewAction) 
            return;
        
        Action action = Actions[ActionId];

        _actionBox = new Box(action.Box);

        AnimatedObject.CurrentAnimation = action.AnimationIndex;
        AnimatedObject.FlipX = (action.Flags & ActionFlags.FlipX) != 0;
        AnimatedObject.FlipY = (action.Flags & ActionFlags.FlipY) != 0;

        if (action.MechModelType != null && this is MovableActor movableActor)
            movableActor.MechModel.Init(action.MechModelType.Value, action.MechModel?.Params);

        NewAction = false;
    }

    public void ReceiveDamage(int damage)
    {
        if (IsInvulnerable)
            return;

        if (damage < HitPoints)
            HitPoints -= damage;
        else
            HitPoints = 0;
    }

    public override void Step()
    {
        ChangeAction();
    }

    public PhysicalType GetPhysicalGroundType()
    {
        Box detectionBox = GetDetectionBox();

        // Get the type at the bottom-center
        Vector2 pos = new(detectionBox.Center.X, detectionBox.Bottom);
        PhysicalType centerType = Scene.GetPhysicalType(pos);

        // If the type is angled, then check if the point within the tile is solid
        if (centerType.IsAngledSolid)
            centerType = centerType.IsBlockPointSolid(pos) ? PhysicalTypeValue.Solid : PhysicalTypeValue.None;

        // Return if the type is solid
        if (centerType.IsSolid) 
            return centerType;

        // Get the type one tile to the left from the center
        pos -= new Vector2(Tile.Size, 0);
        PhysicalType leftType = Scene.GetPhysicalType(pos);

        if (!leftType.IsAngledSolid)
        {
            // Get the type all the way to the left
            pos.X = detectionBox.Left;
            leftType = Scene.GetPhysicalType(pos);

            // If the type is fully solid then we check the tile above it. If it's
            // also fully solid then it's a wall, so we set the type to none.
            if (leftType.IsFullySolid)
            {
                pos -= new Vector2(0, Tile.Size);
                if (Scene.GetPhysicalType(pos).IsFullySolid)
                    leftType = PhysicalTypeValue.None;
            }
        }

        // Get the type one tile to the right from the center
        pos = new Vector2(detectionBox.Center.X + Tile.Size, detectionBox.Bottom);
        PhysicalType rightType = Scene.GetPhysicalType(pos);

        if (!rightType.IsAngledSolid)
        {
            // Get the type all the way to the right
            pos.X = detectionBox.Right;
            rightType = Scene.GetPhysicalType(pos);

            // If the type is fully solid then we check the tile above it. If it's
            // also fully solid then it's a wall, so we set the type to none.
            if (rightType.IsFullySolid)
            {
                pos -= new Vector2(0, Tile.Size);

                if (Scene.GetPhysicalType(pos).IsFullySolid)
                    rightType = PhysicalTypeValue.None;
            }
        }

        // If any of the types are angled solid then return empty type
        if (leftType.IsAngledSolid || rightType.IsAngledSolid)
            return PhysicalTypeValue.None;

        // Return types if fully solid, otherwise none
        if (rightType.IsFullySolid)
            return rightType;
        else if (leftType.IsFullySolid)
            return leftType;
        else
            return PhysicalTypeValue.None;
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        Box detectionBox = GetDetectionBox();
        if (Scene.Camera.IsDebugBoxFramed(_debugDetectionBoxAObject, detectionBox.Position))
        {
            _debugDetectionBoxAObject.Size = detectionBox.Size;
            animationPlayer.PlayFront(_debugDetectionBoxAObject);
        }

        Box actionBox = GetActionBox();
        if (Scene.Camera.IsDebugBoxFramed(_debugActionBoxAObject, actionBox.Position))
        {
            _debugActionBoxAObject.Size = actionBox.Size;
            animationPlayer.PlayFront(_debugActionBoxAObject);
        }
    }

    public override void DrawDebugLayout(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        base.DrawDebugLayout(debugLayout, textureManager);

        int hp = HitPoints;
        if (ImGui.InputInt("HitPoints", ref hp))
            HitPoints = hp;
    }
}