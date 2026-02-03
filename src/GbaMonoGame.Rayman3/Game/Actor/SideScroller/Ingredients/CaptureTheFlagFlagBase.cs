using System.Linq;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class CaptureTheFlagFlagBase : ActionActor
{
    public CaptureTheFlagFlagBase(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Links = actorResource.Links;
        IsFlagTaken = false;
        Timer = 0;
        HasReceivedFlag = false;
        AttachedObject = null;
        TeamPlayer1 = null;
        TeamPlayer2 = null;

        if (MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Solo)
        {
            // Get the number of links
            int linksCount = Links.Count(x => x != null);
            
            // If there are more than 1 links then it's the common base
            IsCommonBase = linksCount > 1;

            // The common base where you get the flags from
            if (IsCommonBase)
            {
                State.SetTo(_Fsm_SoloCommon);
            }
            // A player's base
            else
            {
                // Make sure the player it's linked to is in the game
                if (Links[0] < 4 && Scene.GetGameObject(Links[0].Value).IsEnabled)
                {
                    // Get the player
                    AttachedObject = Scene.GetGameObject<Rayman>(Links[0].Value);

                    // Set the palette from the player
                    MessageRefParam<int> param = new();
                    AttachedObject.ProcessMessage(this, Message.Rayman_GetPlayerPaletteId, param);
                    AnimatedObject.BasePaletteIndex = param.Value;
                    AnimatedObject.Palettes = AttachedObject.AnimatedObject.Palettes;

                    // Hide the flag from the base animation
                    AnimatedObject.DeactivateChannel(0);
                }
                else
                {
                    // Remove the base if linked to a player not in the game
                    ProcessMessage(this, Message.Destroy);
                }
                
                State.SetTo(_Fsm_Solo);
            }
        }
        else
        {
            // Get the players in the team
            TeamPlayer1 = Scene.GetGameObject<Rayman>(Links[0]!.Value);
            TeamPlayer2 = Scene.GetGameObject<Rayman>(Links[1]!.Value);

            // Set the palette from the team players
            MessageRefParam<int> param = new();
            TeamPlayer1.ProcessMessage(this, Message.Rayman_GetPlayerPaletteId, param);
            AnimatedObject.BasePaletteIndex = param.Value;
            AnimatedObject.Palettes = TeamPlayer1.AnimatedObject.Palettes;
            TeamId = param.Value / 2;

            State.SetTo(_Fsm_Teams);
        }
    }

    public ActionActor AttachedObject { get; set; } // Rayman in solo mode, flag in teams mode
    public CaptureTheFlagFlag Flag { get; set; }
    public Rayman TeamPlayer1 { get; set; }
    public Rayman TeamPlayer2 { get; set; }
    public byte?[] Links { get; }
    public bool IsCommonBase { get; }
    public bool IsFlagTaken { get; set; }
    public bool HasReceivedFlag { get; set; }
    public byte Timer { get; set; }
    public int TeamId { get; set; } // Only ever set

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.CaptureTheFlagFlagBase_LinkFlag:
                // Link the flag to this base
                AttachedObject = (CaptureTheFlagFlag)param;
                Flag = (CaptureTheFlagFlag)param;
                return false;

            case Message.CaptureTheFlagFlagBase_ResetFlag:
                // Show the flag in the base animation
                AnimatedObject.ActivateChannel(0);

                // Reset if the flag was taken and add a cooldown
                IsFlagTaken = false;
                Timer = 30;
                return false;

            case Message.CaptureTheFlagFlagBase_GetCapturableFlag:
                MessageRefParam<BaseActor> refParam = (MessageRefParam<BaseActor>)param;
                
                // If the actor is in this team then we return null - you can't capture a flag from your own team!
                if (TeamPlayer1 == refParam.Value || TeamPlayer2 == refParam.Value)
                    refParam.Value = null;
                else
                    refParam.Value = Flag;
                return false;

            default:
                return false;
        }
    }
}