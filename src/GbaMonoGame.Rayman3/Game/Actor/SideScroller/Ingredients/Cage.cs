using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Cage : InteractableActor
{
    public Cage(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialActionId = actorResource.FirstActionId == 0 ? 0 : 6;
        PrevHitPoints = HitPoints;
        
        CageId = GameInfo.GetCageId();

        State.SetTo(Fsm_Idle);

        if (GameInfo.IsCageDead(CageId, GameInfo.MapId))
            ProcessMessage(this, Message.Destroy);
    }

    public int CageId { get; }
    public int InitialActionId { get; }

    public int PrevHitPoints { get; set; }
    public int Timer { get; set; }
    public int HitAction { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Damaged:
                BaseActor actor = (BaseActor)param;
                HitAction = actor.IsFacingLeft ? 3 : 0;
                State.MoveTo(Fsm_Damaged);
                HitPoints--;
                return false;

            case Message.Hit:
                RaymanBody raymanBody = (RaymanBody)param;

                HitAction = raymanBody.IsFacingLeft ? 3 : 0;

                if (raymanBody.BodyPartType is RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist)
                {
                    State.MoveTo(Fsm_Damaged);
                    HitPoints--;
                }
                return false;

            default:
                return false;
        }
    }
}