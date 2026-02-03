using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Piranha : MovableActor
{
    public Piranha(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        InitPos = Position;
        State.SetTo(_Fsm_Wait);
    }

    public Vector2 InitPos { get; }
    public int Timer { get; set; }
    public bool ShouldDraw { get; set; }

    private void SpawnSplash()
    {
        WaterSplash waterSplash = Scene.CreateProjectile<WaterSplash>(ActorType.WaterSplash);
        if (waterSplash != null)
            waterSplash.Position = Position;
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (ShouldDraw)
            base.Draw(animationPlayer, forceDraw);
    }
}