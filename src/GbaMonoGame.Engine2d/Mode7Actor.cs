using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using ImGuiNET;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GbaMonoGame.Engine2d;

public abstract class Mode7Actor : MovableActor
{
    protected Mode7Actor(int instanceId, Scene2D scene, ActorResource actorResource)
        : this(instanceId, scene, actorResource, new AnimatedObject(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic)) { }

    protected Mode7Actor(int instanceId, Scene2D scene, ActorResource actorResource, AnimatedObject animatedObject)
        : base(instanceId, scene, actorResource, animatedObject)
    {
        IsAffine = true;
        Direction = 0;
        ZPos = 0;
        RenderHeight = 32;
        AnimatedObject.BgPriority = 0;

        _debugPositionPointProjectedAObject = new DebugPointAObject()
        {
            Color = DebugBoxColor.PositionPointProjected,
            RenderContext = Scene.RenderContext,
        };
    }

    private readonly DebugPointAObject _debugPositionPointProjectedAObject;

    public float ZPos { get; set; }
    public bool IsAffine { get; set; }
    public int RenderHeight { get; set; }
    public float Direction { get; set; } // 0-256
    public float CamAngle { get; set; }

    public float GetCamDirection()
    {
        return MathHelpers.Mod(-128 - CamAngle - Direction, 256);
    }

    // TODO: Clean up
    public void SetMode7DirectionalAction(int baseActionId, int param_3)
    {
        int camDir = (int)MathF.Round(GetCamDirection());
        int newActionId = MathHelpers.Mod(camDir + (256 >> MathHelpers.Mod(param_3 + 1, 256)), 256) >> MathHelpers.Mod(8 - param_3, 256);

        newActionId += baseActionId;

        if (newActionId != ActionId)
            ActionId = newActionId;
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        // Get the 3D position
        Vector3 actorPos = new(Position, 0);

        // Project to the screen
        Vector3 screenPos = cam.Project(actorPos);

        // Set the screen position
        _debugPositionPointProjectedAObject.ScreenPos = new Vector2(screenPos.X, screenPos.Y);

        animationPlayer.PlayFront(_debugPositionPointProjectedAObject);
    }

    public override void DrawDebugLayout(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        base.DrawDebugLayout(debugLayout, textureManager);

        float direction = Direction;
        if (ImGui.SliderFloat("Direction", ref direction, 0, 256))
            Direction = direction;
    }
}