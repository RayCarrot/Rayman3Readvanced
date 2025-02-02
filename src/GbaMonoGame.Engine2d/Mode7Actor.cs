using System;
using GbaMonoGame.AnimEngine;

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
    }

    public short ZPos { get; set; }
    public bool IsAffine { get; set; }
    public byte RenderHeight { get; set; }
    public float Direction { get; set; }
    public float CamAngle { get; set; }

    public float GetCamDirection()
    {
        return -128 - CamAngle - Direction;
    }

    // TODO: Clean up
    public void SetMode7DirectionalAction(int param_2, int param_3)
    {
        int camDir = (int)MathF.Round(GetCamDirection());
        int newActionId = (int)(param_2 + ((camDir + (256 >> (param_3 + 1 & 0xFF)) & 0xffU) >> (8 - param_3 & 0xFF)));

        if (newActionId != ActionId)
            ActionId = newActionId;
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        // TODO: Implement
    }
}