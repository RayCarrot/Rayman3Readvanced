using System;
using ImGuiNET;

namespace GbaMonoGame.Engine2d;

public abstract class Mode7Actor : MovableActor
{
    protected Mode7Actor(int instanceId, Scene2D scene, ActorResource actorResource)
        : this(instanceId, scene, actorResource, new AnimatedObject(actorResource.Model.AnimatedObject, actorResource.IsAnimatedObjectDynamic)) { }

    protected Mode7Actor(int instanceId, Scene2D scene, ActorResource actorResource, AnimatedObject animatedObject)
        : base(instanceId, scene, actorResource, animatedObject)
    {
        IsAffine = true;
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 32;
        AnimatedObject.BgPriority = 0;
    }

    // Custom so we can define an alpha without modifying the animated object (since
    // that gets overriden by fading out the object as it exists the view)
    public AlphaCoefficient Alpha { get; set; } = AlphaCoefficient.Max;

    public float ZPos { get; set; }
    public bool IsAffine { get; set; }
    public int RenderHeight { get; set; }
    public Angle256 Direction { get; set; }
    public Angle256 CamAngle { get; set; }

    public Angle256 GetCamDirection()
    {
        return -128 - CamAngle - Direction;
    }

    public void SetMode7DirectionalAction(int baseActionId, int size)
    {
        int camDir = (int)MathF.Round(GetCamDirection());

        // NOTE: No idea what this is doing, but it works. Would be nice to rewrite in a cleaner way using floats though.
        int newActionId = MathHelpers.Mod(camDir + (256 >> MathHelpers.Mod(size + 1, 256)), 256) >> MathHelpers.Mod(8 - size, 256);

        newActionId += baseActionId;

        if (newActionId != ActionId)
            ActionId = newActionId;
    }

    public override void DrawDebugLayout(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        base.DrawDebugLayout(debugLayout, textureManager);

        float direction = Direction;
        if (ImGui.SliderFloat("Direction", ref direction, 0, 256))
            Direction = direction;
    }
}