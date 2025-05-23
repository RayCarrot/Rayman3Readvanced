﻿using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class BluePirate : PirateBaseActor
{
    public BluePirate(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
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
        State.SetTo(Fsm_Fall);
        ChangeAction();
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