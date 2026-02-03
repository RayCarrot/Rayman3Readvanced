using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class JanoSkullPlatform : MovableActor
{
    public JanoSkullPlatform(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Jano = Scene.GetGameObject<Jano>(1);
        SkullPlatformIndex = 0;
        State.SetTo(_Fsm_Move);
    }

    public Jano Jano { get; }
    public float TargetY { get; set; }
    public int SkullPlatformIndex { get; set; }
    public ushort Timer { get; set; }

    private void SpawnHitEffect()
    {
        RaymanBody body = Scene.CreateProjectile<RaymanBody>(ActorType.RaymanBody);

        if (body != null)
        {
            body.ActionId = RaymanBody.Action.HitEffect;
            body.BodyPartType = RaymanBody.RaymanBodyPartType.HitEffect;
            body.Position = Position + new Vector2(16, 0);
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_CollideWithSameType:
                State.MoveTo(_Fsm_FallDown);
                ActionId = Action.FallDown;
                return false;

            default:
                return false;
        }
    }
}