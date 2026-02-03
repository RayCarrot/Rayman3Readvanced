using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

// Original name: Grille
[GenerateFsmFields]
public sealed partial class Gate : InteractableActor
{
    public Gate(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        IsOpen = false;
        AnimatedObject.ObjPriority = 61;

        RemainingSwitches = (Action)actorResource.FirstActionId switch
        {
            Action.Init_4Switches_Right or Action.Init_4Switches_Left => 4,
            Action.Init_3Switches_Right or Action.Init_3Switches_Left => 3,
            _ => 1
        };

        State.SetTo(_Fsm_Closed);
    }

    public bool IsOpen { get; set; }
    public byte RemainingSwitches { get; set; }

    public override void Init(ActorResource actorResource)
    {
        if (actorResource.Links[0] != null && 
            !Scene.GetGameObject(actorResource.Links[0].Value).IsEnabled)
        {
            IsOpen = true;
            State.SetTo(_Fsm_Closed);

            if (actorResource.Links[1] != null)
                Scene.GetGameObject<Switch>(actorResource.Links[1].Value).SetToActivated();
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Gate_Open:
                RemainingSwitches--;

                if (RemainingSwitches == 0)
                {
                    IsOpen = true;
                    LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win1);
                }
                return false;

            // Unused
            case Message.Gate_Close:
                IsOpen = false;
                return false;

            default:
                return false;
        }
    }
}