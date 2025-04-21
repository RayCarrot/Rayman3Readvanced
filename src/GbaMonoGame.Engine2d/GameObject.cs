using GbaMonoGame.AnimEngine;
using ImGuiNET;

namespace GbaMonoGame.Engine2d;

public abstract class GameObject : Object
{
    protected GameObject(int instanceId, Scene2D scene, GameObjectResource gameObjectResource)
    {
        InstanceId = instanceId;
        Scene = scene;
        Position = gameObjectResource.Pos.ToVector2();

        IsEnabled = gameObjectResource.IsEnabled;
        IsAwake = gameObjectResource.IsAwake;
        Flag_2 = false;
        IsProjectile = gameObjectResource.IsProjectile;
        ResurrectsImmediately = gameObjectResource.ResurrectsImmediately;
        ResurrectsLater = gameObjectResource.ResurrectsLater;
        Flag_6 = gameObjectResource.Flag_6;
        Flag_7 = gameObjectResource.Flag_7;

        _debugPositionPointAObject = new DebugPointAObject()
        {
            Color = DebugBoxColor.PositionPoint,
            RenderContext = Scene.RenderContext,
        };
    }

    private readonly DebugPointAObject _debugPositionPointAObject;

    public int InstanceId { get; }
    public Scene2D Scene { get; }
    public Vector2 Position { get; set; }

    // Flags
    public bool IsEnabled { get; set; }
    public bool IsAwake { get; set; }
    public bool Flag_2 { get; set; } // Unused and unreferenced in Rayman 3
    public bool IsProjectile { get; set; }
    public bool ResurrectsImmediately { get; set; }
    public bool ResurrectsLater { get; set; }
    public bool Flag_6 { get; set; } // Unused in Rayman 3
    public bool Flag_7 { get; set; } // Unused in Rayman 3

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        switch (message)
        {
            case Message.None:
                return true;

            case Message.WakeUp:
                IsAwake = true;
                return true;

            case Message.Sleep:
                IsAwake = false;
                return true;

            case Message.Destroy:
                IsEnabled = false;
                return true;

            case Message.Resurrect:
                IsEnabled = true;
                return true;

            case Message.ResurrectWakeUp:
                IsEnabled = true;
                IsAwake = true;
                return true;

            default:
                return false;
        }
    }

    public virtual void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        if (Scene.Camera.IsDebugBoxFramed(_debugPositionPointAObject, Position))
            animationPlayer.PlayFront(_debugPositionPointAObject);
    }

    public override void DrawDebugLayout(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        ImGui.Text($"Projectile: {IsProjectile}");
        base.DrawDebugLayout(debugLayout, textureManager);
    }
}