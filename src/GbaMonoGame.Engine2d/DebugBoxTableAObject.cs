using System;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Engine2d;

public class DebugBoxTableAObject : AObject
{
    public DebugBoxTableAObject(InteractableActor actor)
    {
        Actor = actor;
        AttackBox = new DebugBoxAObject()
        {
            Color = DebugBoxColor.AttackBox,
        };
        VulnerabilityBox = new DebugBoxAObject()
        {
            Color = DebugBoxColor.VulnerabilityBox,
        };
    }

    public InteractableActor Actor { get; }
    public DebugBoxAObject AttackBox { get; }
    public DebugBoxAObject VulnerabilityBox { get; }

    public override void Execute(Action<short> soundEventCallback)
    {
        if (AttackBox.RenderContext != RenderContext)
            AttackBox.RenderContext = RenderContext;

        Box attackBox = Actor.GetAttackBox();
        if (Actor.Scene.Camera.IsDebugBoxFramed(AttackBox, attackBox.Position))
        {
            AttackBox.Size = attackBox.Size;
            AttackBox.Execute(soundEventCallback);
        }

        if (VulnerabilityBox.RenderContext != RenderContext)
            VulnerabilityBox.RenderContext = RenderContext;

        Box vulnerabilityBox = Actor.GetVulnerabilityBox();
        if (Actor.Scene.Camera.IsDebugBoxFramed(VulnerabilityBox, vulnerabilityBox.Position))
        {
            VulnerabilityBox.Size = vulnerabilityBox.Size;
            VulnerabilityBox.Execute(soundEventCallback);
        }
    }
}