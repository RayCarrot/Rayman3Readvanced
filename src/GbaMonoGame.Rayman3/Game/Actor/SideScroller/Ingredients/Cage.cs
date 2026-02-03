using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Cage : InteractableActor
{
    public Cage(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        IsGrounded = actorResource.FirstActionId == 0;
        PrevHitPoints = HitPoints;
        
        CageId = GameInfo.GetCageId();

        State.SetTo(_Fsm_Idle);

        if (GameInfo.IsCageDead(CageId, GameInfo.MapId))
            ProcessMessage(this, Message.Destroy);
    }

    public int CageId { get; }
    public bool IsGrounded { get; } // NOTE: In the original game this is a base action id of 0 or 6

    public int PrevHitPoints { get; set; }
    public int Timer { get; set; }
    public bool IsHitToLeft { get; set; } // NOTE: In the original game this is a base action id of 0 or 3

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_Hurt:
                BaseActor actor = (BaseActor)param;
                IsHitToLeft = actor.IsFacingLeft;
                State.MoveTo(_Fsm_Damaged);
                HitPoints--;
                return false;

            case Message.Actor_Hit:
                RaymanBody raymanBody = (RaymanBody)param;

                IsHitToLeft = raymanBody.IsFacingLeft;

                if (raymanBody.BodyPartType is RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist)
                {
                    State.MoveTo(_Fsm_Damaged);
                    HitPoints--;
                }
                return false;

            default:
                return false;
        }
    }
}