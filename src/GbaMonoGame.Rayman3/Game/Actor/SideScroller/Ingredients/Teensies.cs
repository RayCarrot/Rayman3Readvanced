using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: PetitsEtres
public sealed partial class Teensies : ActionActor
{
    public Teensies(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        if (ActionId is
            Action.Init_Master_Right or Action.Init_Master_Left or
            Action.Init_World1_Right or Action.Init_World1_Left or
            Action.Init_World2_Right or Action.Init_World2_Left or
            Action.Init_World3_Right or Action.Init_World3_Left or
            Action.Init_World4_Right or Action.Init_World4_Left)
        {
            InitialActionId = ActionId;

            if ((ActionId is Action.Init_World1_Right or Action.Init_World1_Left && GameInfo.PersistentInfo.UnlockedWorld2) ||
                (ActionId is Action.Init_World2_Right or Action.Init_World2_Left && GameInfo.PersistentInfo.UnlockedWorld3) ||
                (ActionId is Action.Init_World3_Right or Action.Init_World3_Left && GameInfo.PersistentInfo.UnlockedWorld4) ||
                (ActionId is Action.Init_World4_Right or Action.Init_World4_Left && GameInfo.PersistentInfo.UnlockedFinalBoss))
            {
                ProcessMessage(this, Message.Destroy);
            }
            else
            {
                ActionId = IsFacingRight ? Action.Init_Master_Right : Action.Init_Master_Left;
                State.SetTo(Fsm_WaitMaster);
            }
        }
        else if (ActionId is Action.Init_Victory_Right or Action.Init_Victory_Left)
        {
            State.SetTo(Fsm_VictoryDance);
        }
        else
        {
            State.SetTo(Fsm_Idle);
        }
    }

    public Action InitialActionId { get; }

    public bool IsMovingOutTextBox { get; set; }
    public bool HasSetTextBox { get; set; }
    public TextBoxDialog TextBox { get; set; }

    public bool SavedBlockPause { get; set; }

    private void SetMasterAction()
    {
        if (IsActionFinished)
            ActionId = Random.GetNumber(5) * 2 + (IsFacingRight ? Action.Master1_Right : Action.Master1_Left);
    }

    private bool IsMapRequirementFulfilled()
    {
        if (InitialActionId is Action.Init_World1_Right or Action.Init_World1_Left)
            return GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfBigTree_M2;
        else if (InitialActionId is Action.Init_World2_Right or Action.Init_World2_Left)
            return GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.MarshAwakening2;
        else if (InitialActionId is Action.Init_World3_Right or Action.Init_World3_Left)
            return GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfRockAndLava_M3;
        else if (InitialActionId is Action.Init_World4_Right or Action.Init_World4_Left)
            return GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.PirateShip_M2;
        else
            throw new Exception("Invalid initial action id for teensies");
    }

    private bool IsEnoughCagesTaken()
    {
        if (InitialActionId is Action.Init_World1_Right or Action.Init_World1_Left)
        {
            if (GameInfo.GetTotalDeadCages() >= 5)
            {
                GameInfo.PersistentInfo.UnlockedWorld2 = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (InitialActionId is Action.Init_World2_Right or Action.Init_World2_Left)
        {
            if (GameInfo.GetTotalDeadCages() >= 10)
            {
                GameInfo.PersistentInfo.UnlockedWorld3 = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (InitialActionId is Action.Init_World3_Right or Action.Init_World3_Left)
        {
            if (GameInfo.GetTotalDeadCages() >= 15)
            {
                GameInfo.PersistentInfo.UnlockedWorld4 = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (InitialActionId is Action.Init_World4_Right or Action.Init_World4_Left)
        {
            if (GameInfo.GetTotalDeadCages() >= 20)
            {
                GameInfo.PersistentInfo.UnlockedFinalBoss = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            throw new Exception("Invalid initial action id for teensies");
        }
    }

    private void SetRequirementMetText()
    {
        switch (InitialActionId)
        {
            case Action.Init_World1_Right or Action.Init_World1_Left:
                TextBox.SetText(1);
                break;

            case Action.Init_World2_Right or Action.Init_World2_Left:
                TextBox.SetText(4);
                break;

            case Action.Init_World3_Right or Action.Init_World3_Left:
                TextBox.SetText(7);
                break;

            case Action.Init_World4_Right or Action.Init_World4_Left:
                TextBox.SetText(10);
                break;
        }
    }

    private void SetRequirementNotMetText()
    {
        if (IsMapRequirementFulfilled())
        {
            switch (InitialActionId)
            {
                case Action.Init_World1_Right or Action.Init_World1_Left:
                    TextBox.SetText(2);
                    break;

                case Action.Init_World2_Right or Action.Init_World2_Left:
                    TextBox.SetText(5);
                    break;

                case Action.Init_World3_Right or Action.Init_World3_Left:
                    TextBox.SetText(8);
                    break;

                case Action.Init_World4_Right or Action.Init_World4_Left:
                    TextBox.SetText(11);
                    break;
            }
        }
        else
        {
            switch (InitialActionId)
            {
                case Action.Init_World1_Right or Action.Init_World1_Left:
                    TextBox.SetText(3);
                    break;

                case Action.Init_World2_Right or Action.Init_World2_Left:
                    TextBox.SetText(6);
                    break;

                case Action.Init_World3_Right or Action.Init_World3_Left:
                    TextBox.SetText(9);
                    break;

                case Action.Init_World4_Right or Action.Init_World4_Left:
                    TextBox.SetText(12);
                    break;
            }
        }
    }

    private bool HasLeftMainActorView()
    {
        Box viewBox = GetViewBox();
        viewBox = new Box(viewBox.MinX + 45, viewBox.MinY, viewBox.MaxX - 45, viewBox.MaxY);

        return !Scene.MainActor.GetDetectionBox().Intersects(viewBox);
    }
}